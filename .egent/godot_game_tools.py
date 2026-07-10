"""Godot 运行时启动与脚本执行工具。"""

from __future__ import annotations

import json
import re
import subprocess
import sys
import threading
from datetime import datetime
from pathlib import Path

_PROJECT_ROOT = Path(__file__).resolve().parent.parent
_EGENT_DIR = Path(__file__).resolve().parent
_IDE_MCP_DIR = _PROJECT_ROOT / "addons" / "godot_runtime_mcp"
_GODOT_EXE = _PROJECT_ROOT / ".engine" / ".engine.exe"
_LOG_DIR = _EGENT_DIR / ".logs"
_PORT_PATTERN = re.compile(r"<<<GAME_MCP::PORT=(\d+)>>>")

if str(_IDE_MCP_DIR) not in sys.path:
    sys.path.insert(0, str(_IDE_MCP_DIR))

from agent_mcp import send_http  # pylint: disable=import-error,wrong-import-position


def launch_game_session() -> tuple[int, subprocess.Popen, Path]:
    """启动 Godot 游戏，返回 (MCP 端口, 子进程, 日志路径)。"""
    if not _GODOT_EXE.is_file():
        raise RuntimeError(f"找不到 Godot 可执行文件: {_GODOT_EXE}")

    try:
        reimport_result = subprocess.run(
            [str(_GODOT_EXE), "--path", str(_PROJECT_ROOT), "--headless", "--import"],
            cwd=_PROJECT_ROOT,
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            check=False,
            timeout=60.0,
        )
    except subprocess.TimeoutExpired as error:
        raise RuntimeError(
            "资源重导入超时 (60s)",
        ) from error
    if reimport_result.returncode != 0:
        output = f"{reimport_result.stdout}\n{reimport_result.stderr}".strip()
        raise RuntimeError(f"资源重导入失败\n{output}")

    _LOG_DIR.mkdir(parents=True, exist_ok=True)
    log_path = _LOG_DIR / f"godot_{datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}.log"

    process = subprocess.Popen(  # pylint: disable=consider-using-with
        [str(_GODOT_EXE), "--path", str(_PROJECT_ROOT)],
        cwd=_PROJECT_ROOT,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding="utf-8",
        errors="replace",
        bufsize=1,
    )

    detected_port: int | None = None
    port_ready = threading.Event()

    def drain_stdout() -> None:
        nonlocal detected_port
        assert process.stdout is not None
        with log_path.open("w", encoding="utf-8") as log_file:
            for line in process.stdout:
                log_file.write(line)
                log_file.flush()
                if detected_port is not None:
                    continue
                port_match = _PORT_PATTERN.search(line)
                if port_match is None:
                    continue
                detected_port = int(port_match.group(1))
                port_ready.set()

    reader = threading.Thread(target=drain_stdout, daemon=True)
    reader.start()

    deadline = datetime.now().timestamp() + 120.0
    while datetime.now().timestamp() < deadline:
        if port_ready.wait(timeout=0.2):
            assert detected_port is not None
            return detected_port, process, log_path
        if process.poll() is not None:
            reader.join(timeout=1)
            if detected_port is not None:
                return detected_port, process, log_path
            raise RuntimeError(
                f"游戏在输出 MCP 端口前退出 (code={process.returncode})，"
                f"日志: {log_path.as_posix()}"
            )

    process.kill()
    reader.join(timeout=1)
    raise RuntimeError(
        "等待 MCP 端口超时 (120s)，"
        f"日志: {log_path.as_posix()}"
    )


def launch_game() -> str:
    """启动 Godot 游戏并返回 MCP 端口号与日志路径。"""
    port, _process, log_path = launch_game_session()
    return json.dumps(
        {"port": port, "log_path": log_path.as_posix()},
        ensure_ascii=False,
        indent=2,
    )


def _require_white_test_script(script_path: str) -> str:
    """读取 white_tests 下的 .gd 脚本；路径非法或文件不存在时抛异常。"""
    normalized_path = script_path.strip().replace("\\", "/")
    if not normalized_path:
        raise ValueError("脚本路径不能为空")
    candidate = Path(normalized_path)
    if not candidate.is_absolute():
        candidate = _PROJECT_ROOT / candidate
    resolved = candidate.resolve()
    white_tests_resolved = (_PROJECT_ROOT / "tests" / "white_tests").resolve()
    try:
        resolved.relative_to(white_tests_resolved)
    except ValueError as path_error:
        raise ValueError(
            f"脚本路径无效或不存在，须为 tests/white_tests 下的 .gd 文件: {script_path}"
        ) from path_error
    if resolved.suffix != ".gd" or not resolved.is_file():
        raise FileNotFoundError(
            f"脚本路径无效或不存在，须为 tests/white_tests 下的 .gd 文件: {script_path}"
        )
    script_source = resolved.read_text(encoding="utf-8")
    if not script_source.strip():
        raise ValueError(f"脚本为空: {script_path}")
    return script_source


def run_gdscript(port: int, script_path: str, *, timeout: float) -> str:
    """在运行中的 Godot 实例里执行 tests/white_tests 下的白盒测试脚本。

    @param script_path: tests/white_tests 下的 .gd 路径，如 tests/white_tests/test_pause.gd
    @param timeout HTTP 请求超时秒数，调用方必须显式指定。
    """
    script_source = _require_white_test_script(script_path)
    result = send_http(port, script_source, timeout_seconds=timeout)
    return json.dumps(result, ensure_ascii=False, indent=2)
