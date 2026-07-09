"""开发编码工作流。"""

from __future__ import annotations

import json
import subprocess
import sys
from collections.abc import Callable
from pathlib import Path

import _common
import egent
import egent.agent
import egent.builtin_tools.path_validator
import godot_game_tools

_PROJECT_ROOT = Path(__file__).resolve().parent.parent
if str(_PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(_PROJECT_ROOT))

from tests.run_tests import run_regression

_RUN_REGRESSION_BAT = _PROJECT_ROOT / "run-regression.bat"
_REGRESSION_BAT_TIMEOUT_SECONDS = 120.0


def _terminate_tracked_processes(processes: list[subprocess.Popen]) -> None:
    for process in processes:
        if process.poll() is None:
            process.kill()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                pass


def _make_launch_game_tool(
    tracked_processes: list[subprocess.Popen],
) -> Callable[[], str]:
    def launch_game() -> str:
        """启动 Godot 游戏并返回 MCP 端口号与日志路径。"""
        try:
            port, process, log_path = godot_game_tools.launch_game_session()
        except RuntimeError as error:
            return f"error: {error}"
        tracked_processes.append(process)
        return json.dumps(
            {"port": port, "log_path": log_path.as_posix()},
            ensure_ascii=False,
            indent=2,
        )

    return launch_game


class CodingGaveUp(Exception):
    """开发者主动放弃任务。"""

    def __init__(self, reason: str) -> None:
        self.reason = reason
        super().__init__(reason)


def _run_regression_batch() -> tuple[bool, str]:
    """运行回归测试批处理，返回 (是否通过, 输出)。"""
    try:
        test_result = subprocess.run(
            ["cmd", "/c", str(_RUN_REGRESSION_BAT)],
            cwd=_PROJECT_ROOT,
            capture_output=True,
            text=True,
            check=False,
            timeout=_REGRESSION_BAT_TIMEOUT_SECONDS,
        )
    except subprocess.TimeoutExpired:
        return False, f"回归测试超时（{_REGRESSION_BAT_TIMEOUT_SECONDS:.0f}s）"
    if test_result.returncode == 0:
        return True, "测试通过"
    return False, f"{test_result.stdout}\n{test_result.stderr}".strip()


async def coding(
    coder: egent.agent.Agent,
    prompt: str,
    *,
    custom_path_permissions: egent.builtin_tools.path_validator.PathPermissions | None = None,
) -> tuple[bool, str]:
    """执行开发：实现、优化、跑回归测试；最多重试直至通过。"""
    tracked_processes: list[subprocess.Popen] = []
    launch_game_tool = _make_launch_game_tool(tracked_processes)

    def run_regression_test(spec: str) -> str:
        """运行指定回归测试套件并返回输出。

        @param spec: 测试套件：all（全部）、smoke
        """
        _, output = run_regression(spec)
        return output

    if custom_path_permissions is not None:
        coder.path_permissions = custom_path_permissions
    elif coder.path_permissions is None:
        coder.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
            root=Path.cwd().resolve(),
            discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=("**",),
                blacklist=(
                    "**/*.pyc",
                    "**/.pytest_cache",
                    "**/.ruff_cache",
                    "**/__pycache__",
                    "**/.agents",
                    "**/.cursor",
                    "**/.egent",
                    "**/.engine",
                    "**/.export",
                    "**/.git",
                    "**/.godot",
                    "**/.logs",
                ),
            ),
            readable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=("**",),
                blacklist=("**/.model.toml",),
            ),
            editable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=("**",),
                blacklist=(
                    "**/.model.toml",
                    "**/.agents",
                    "**/.cursor",
                    "**/.egent",
                    "**/.engine",
                    "**/.export",
                    "**/.git",
                    "**/.godot",
                    "**/.logs",
                ),
            ),
        )

    coder.add_message(
        "system",
        f"{prompt}"
        "## 实现原则\n"
        "不要追求最小 diff，应追求最优雅、最易维护的实现。\n"
        "若现有结构阻碍正确性、可读性或可扩展性，主动重构相关代码；宁可多做一步，也不留补丁式或凑合式修改。"
        "编码完整后使用run_regression_test进行你的专项测试(不需要全跑.跑你相关的专项测试即可.你提交之后会有专门的流程跑测试)"
    )

    last_failure_output = ""
    for _ in range(5):
        try:
            coder.tools = [
                *_common.GIT_READ_ONLY_TOOLS,
                run_regression_test,
                launch_game_tool,
                godot_game_tools.run_gdscript,
            ]
            submitted = await coder.request_submit({
                "success": (bool, "true表示任务完成,false表示放弃"),
                "reason": (str, "如果放弃，填放弃原因,例如需求不合理,或者无法实现等。否则填一个减号`-`"),
            })
        finally:
            _terminate_tracked_processes(tracked_processes)
            tracked_processes.clear()

        if not submitted["success"]:
            raise CodingGaveUp(submitted["reason"])

        coder.add_message(
            "system",
            "编码已完成。请使用 code-optimize技能优化代码"
        )
        coder.tools = list(_common.GIT_READ_ONLY_TOOLS)
        await coder.request()

        passed, last_failure_output = _run_regression_batch()
        if passed:
            return True, ""

        coder.add_message(
            "system",
            f"回归测试失败。请修复:\n\n{last_failure_output}\n\n请仔细查看需求:\n\n{prompt}",
        )
    coder.add_message(
        "system",
        "回归测试连续失败。开发计划暂停。"
        "请总结本次开发工作遇到的问题。"
    )
    coder.tools = []
    await coder.request()
    return False, coder.last_message
