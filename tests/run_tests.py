"""启动 Godot 并执行回归测试（不经过 MCP）。"""

from __future__ import annotations

import argparse
import re
import subprocess
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

_PROJECT_ROOT = Path(__file__).resolve().parent.parent
if str(_PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(_PROJECT_ROOT))

_ENGINE_PREPARE_BAT = _PROJECT_ROOT / ".engine-prepare.bat"
_GODOT_EXE = _PROJECT_ROOT / ".engine" / ".engine.exe"
_DEFAULT_SPEC = "all"
_VALID_SPECS = frozenset({"all", "smoke"})
_INDIVIDUAL_SPECS = ("smoke",)
_SMOKE_TIMEOUT_SEC = 120.0
_GODOT_ISSUE_LINE = re.compile(r"(?i)^(WARNING|ERROR|SCRIPT ERROR)\s*:")
_POLL_INTERVAL_SEC = 0.02


def _is_engine_issue_line(line: str) -> bool:
    stripped = line.lstrip()
    return bool(stripped) and _GODOT_ISSUE_LINE.match(stripped) is not None


def _find_first_engine_issue(text: str, stream_name: str) -> str | None:
    for line in text.splitlines():
        if _is_engine_issue_line(line):
            return f"{stream_name} 出现引擎告警: {line.rstrip()}"
    return None


def _scan_collected_output(stdout_text: str, stderr_text: str) -> str | None:
    return _find_first_engine_issue(stdout_text, "stdout") or _find_first_engine_issue(
        stderr_text,
        "stderr",
    )


def _prepare_engine() -> str | None:
    try:
        prepare_result = subprocess.run(
            ["cmd", "/c", str(_ENGINE_PREPARE_BAT)],
            cwd=_PROJECT_ROOT,
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            check=False,
            timeout=60.0,
        )
    except subprocess.TimeoutExpired:
        return "error: 引擎准备超时 (60s)"
    if prepare_result.returncode != 0:
        output = f"{prepare_result.stdout}\n{prepare_result.stderr}".strip()
        return f"error: 引擎准备失败\n{output}"
    if not _GODOT_EXE.is_file():
        return f"error: 找不到 Godot 可执行文件: {_GODOT_EXE}"
    return None


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="运行 RealismCombat 回归测试")
    parser.add_argument(
        "spec",
        nargs="?",
        default=_DEFAULT_SPEC,
        choices=sorted(_VALID_SPECS),
        help="测试套件：all（默认，并发）、smoke",
    )
    return parser


def _spec_timeout_sec(spec: str) -> float:
    if spec == "smoke":
        return _SMOKE_TIMEOUT_SEC
    return 60.0


def _run_subprocess_with_output_guard(
    command: list[str],
    timeout_sec: float,
) -> tuple[int | None, str, str, str | None]:
    """运行子进程；stdout/stderr 中的 WARNING/ERROR 会立即终止。"""
    process = subprocess.Popen(
        command,
        cwd=_PROJECT_ROOT,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        encoding="utf-8",
        errors="replace",
        bufsize=1,
    )
    stdout_chunks: list[str] = []
    stderr_chunks: list[str] = []
    violation = threading.Event()
    violation_message: list[str] = []

    def report_violation(message: str) -> None:
        if violation.is_set():
            return
        violation_message.append(message)
        violation.set()

    def read_stdout() -> None:
        assert process.stdout is not None
        for line in process.stdout:
            stdout_chunks.append(line)
            if _is_engine_issue_line(line):
                report_violation(f"stdout 出现引擎告警: {line.rstrip()}")

    def read_stderr() -> None:
        assert process.stderr is not None
        for line in process.stderr:
            stderr_chunks.append(line)
            if _is_engine_issue_line(line):
                report_violation(f"stderr 出现引擎告警: {line.rstrip()}")

    stdout_thread = threading.Thread(target=read_stdout, daemon=True)
    stderr_thread = threading.Thread(target=read_stderr, daemon=True)
    stdout_thread.start()
    stderr_thread.start()

    deadline = time.monotonic() + timeout_sec
    timed_out = False
    try:
        while process.poll() is None:
            if violation.is_set():
                process.kill()
                break
            if time.monotonic() >= deadline:
                process.kill()
                timed_out = True
                break
            time.sleep(_POLL_INTERVAL_SEC)
    finally:
        process.wait()
        stdout_thread.join(timeout=2.0)
        stderr_thread.join(timeout=2.0)

    stdout_text = "".join(stdout_chunks).rstrip()
    stderr_text = "".join(stderr_chunks).rstrip()
    if timed_out:
        return process.returncode, stdout_text, stderr_text, f"error: 回归测试超时 ({timeout_sec:.0f}s)"
    if violation.is_set():
        reason = violation_message[0] if violation_message else "未知引擎输出"
        return process.returncode, stdout_text, stderr_text, f"error: {reason}，已终止进程"
    post_scan_issue = _scan_collected_output(stdout_text, stderr_text)
    if post_scan_issue is not None:
        return process.returncode, stdout_text, stderr_text, f"error: {post_scan_issue}"
    return process.returncode, stdout_text, stderr_text, None


def _format_process_output(header: str, stdout_text: str, stderr_text: str, error_message: str | None) -> str:
    output_parts = [header]
    if stdout_text:
        output_parts.append(stdout_text)
    if stderr_text:
        output_parts.append(stderr_text)
    if error_message is not None:
        output_parts.append(error_message)
    return "\n".join(part for part in output_parts if part)


def _run_godot_spec(spec: str) -> tuple[bool, str]:
    """在已准备好的引擎上运行单个回归测试套件。"""
    command = [
        str(_GODOT_EXE),
        "--path",
        str(_PROJECT_ROOT),
        "--",
        f"--regression-test={spec}",
    ]
    header = f"执行回归测试: {spec}"
    return_code, stdout_text, stderr_text, error_message = _run_subprocess_with_output_guard(
        command,
        _spec_timeout_sec(spec),
    )
    output = _format_process_output(header, stdout_text, stderr_text, error_message)
    if error_message is not None:
        return False, output
    if return_code == 0:
        return True, output
    return False, output


def _run_all_concurrent() -> tuple[bool, str]:
    """并发启动多个 Godot 进程，各跑一个测试套件。"""
    results: dict[str, tuple[bool, str]] = {}
    with ThreadPoolExecutor(max_workers=len(_INDIVIDUAL_SPECS)) as executor:
        future_to_spec = {
            executor.submit(_run_godot_spec, spec): spec for spec in _INDIVIDUAL_SPECS
        }
        for future in as_completed(future_to_spec):
            spec = future_to_spec[future]
            results[spec] = future.result()

    all_passed = all(results[spec][0] for spec in _INDIVIDUAL_SPECS)
    output_parts = [f"执行回归测试: all（{len(_INDIVIDUAL_SPECS)} 并发）"]
    for spec in _INDIVIDUAL_SPECS:
        passed, output = results[spec]
        status = "通过" if passed else "失败"
        output_parts.append(f"=== {spec} ({status}) ===")
        output_parts.append(output)
    return all_passed, "\n".join(output_parts)


def run_regression(spec: str = _DEFAULT_SPEC) -> tuple[bool, str]:
    """运行指定回归测试套件，返回 (是否通过, 输出文本)。"""
    if spec not in _VALID_SPECS:
        message = (
            f"error: 无效测试套件 {spec!r}，"
            f"可选: {', '.join(sorted(_VALID_SPECS))}"
        )
        return False, message

    prepare_error = _prepare_engine()
    if prepare_error is not None:
        return False, prepare_error

    if spec == "all":
        return _run_all_concurrent()
    return _run_godot_spec(spec)


def main() -> int:
    arguments = _build_parser().parse_args()
    passed, output = run_regression(arguments.spec)
    if output.startswith("error:"):
        print(output, file=sys.stderr)
        return 1
    if output:
        print(output)
    return 0 if passed else 1


if __name__ == "__main__":
    raise SystemExit(main())
