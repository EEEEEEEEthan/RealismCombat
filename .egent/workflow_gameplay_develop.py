"""游戏玩法开发工作流：编码、验收、白盒测试循环。"""

from __future__ import annotations

import asyncio
import subprocess
import sys
from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator
import godot_game_tools
import workflow_review

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
) -> tuple[bool, str]:
    """执行开发：实现、优化、跑回归测试；最多重试直至通过。"""
    project_root = Path.cwd().resolve().as_posix()
    coder.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
                f"{project_root}/.agents",
                f"{project_root}/.cursor",
                f"{project_root}/.egent",
                f"{project_root}/.engine",
                f"{project_root}/.export",
                f"{project_root}/.git",
                f"{project_root}/.godot",
                f"{project_root}/.logs",
            ),
        ),
        readable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=("*/.model.toml",),
        ),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*/.model.toml",
                "*.pyc",
                "*/.pytest_cache/*",
                "*/.ruff_cache/*",
                "*/__pycache__/*",
                f"{project_root}/.agents/*",
                f"{project_root}/.cursor/*",
                f"{project_root}/.egent/*",
                f"{project_root}/.engine/*",
                f"{project_root}/.export/*",
                f"{project_root}/.git/*",
                f"{project_root}/.godot/*",
                f"{project_root}/.logs/*",
            ),
        ),
    )
    tracked_processes: list[subprocess.Popen] = []

    def run_regression_test(spec: str) -> str:
        """运行指定回归测试套件并返回输出。

        @param spec: 测试套件：all（全部）、smoke
        """
        _, output = run_regression(spec)
        return output

    coder.add_message(
        "system",
        f"{prompt}"
        "## 实现原则\n"
        "不要追求最小 diff，应追求最优雅、最易维护的实现。\n"
        "若现有结构阻碍正确性、可读性或可扩展性，主动重构相关代码；宁可多做一步，也不留补丁式或凑合式修改。"
        "编码完整后使用run_regression_test进行你的专项测试(不需要全跑.跑你相关的专项测试即可.你提交之后会有专门的流程跑测试)",
    )

    last_failure_output = ""
    for _ in range(5):
        try:
            coder.tools = [
                *_common.GIT_READ_ONLY_TOOLS,
                run_regression_test,
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
            "编码已完成。请使用 code-optimize技能优化代码",
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
        "请总结本次开发工作遇到的问题。",
    )
    coder.tools = []
    await coder.request()
    return False, coder.last_message


async def test(prompt: str) -> tuple[bool, str]:
    """通过 run_white_test 白盒校验游戏是否满足需求。"""
    try:
        game_port, game_process, game_log_path = godot_game_tools.launch_game_session()
    except RuntimeError as error:
        return False, str(error)

    try:
        tester = egent.agent.Agent("gpt5")
        project_root = Path.cwd().resolve().as_posix()
        tester.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
            discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(project_root, f"{project_root}/*"),
                blacklist=(
                    "*.pyc",
                    "*/.pytest_cache",
                    "*/.ruff_cache",
                    "*/__pycache__",
                    f"{project_root}/.agents",
                    f"{project_root}/.cursor",
                    f"{project_root}/.egent",
                    f"{project_root}/.engine",
                    f"{project_root}/.export",
                    f"{project_root}/.git",
                    f"{project_root}/.godot",
                    f"{project_root}/.logs",
                    f"{project_root}/tests/regression",
                ),
            ),
            readable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(project_root, f"{project_root}/*"),
                blacklist=(
                    "*/.model.toml",
                    f"{project_root}/tests/regression",
                    f"{project_root}/tests/regression/*",
                ),
            ),
            editable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(
                    f"{project_root}/tests/white_tests",
                    f"{project_root}/tests/white_tests/*",
                ),
            ),
        )
        with conversation_printer.ConversationPrinter(tester):
            tester.add_message(
                "system",
                "你是这个项目的白盒测试员：编写并执行测试脚本，从运行中的游戏实例读取状态，"
                "校验开发成果是否满足需求。"
                "\n\n"
                f"## 运行环境\n"
                f"- 日志 {game_log_path.as_posix()}（Godot stdout/stderr；排障时用 read_file）\n"
                "\n"
                "## 工作流程\n"
                "1. walk_files / git_diff 了解变更与场景结构\n"
                "2. 拆用例清单，在 tests/white_tests 下编写测试脚本（每个脚本只做一件简单的事,用来模拟玩家操作或者查看场景树），"
                "并在 tests/white_tests/index.md 登记脚本功能；优先复用已有脚本与 _common.gd\n"
                f"3. 对端口 {game_port} 调用 run_white_test(script_path=...) 执行测试；"
                "失败查日志，必要时修正脚本后重跑\n"
                "4. 汇总各用例结果（含期望/实际差异），用 submit_task 提交\n"
                "\n"
                "## 测试脚本目录\n"
                "- 所有白盒测试 .gd 脚本写在 tests/white_tests/ 下（你仅有此目录写权限）\n"
                "- tests/white_tests/index.md 记录每个脚本的功能，鼓励复用、避免重复造轮子\n"
                "- 禁止访问 tests/regression（回归测试由其他流程负责）\n"
                "\n"
                "## run_white_test 脚本约定\n"
                "- script_path：tests/white_tests 下的 .gd 路径，如 tests/white_tests/test_pause.gd\n"
                "- 脚本须 extends RefCounted，定义 static func run(scene_tree: SceneTree) -> Variant\n"
                '- 返回可 JSON 序列化的断言数据（如 {"ok": true, ...}）\n'
                "- 可 preload res://tests/white_tests/_common.gd 复用 start_new_game / wait_until 等工具\n"
                "- 每个脚本只做一件简单的事；复杂场景拆成多个脚本组合调用\n"
                "\n"
                "### 示例 1：暂停游戏\n"
                "```gdscript\n"
                "extends RefCounted\n"
                "\n"
                'const _Common := preload("res://tests/white_tests/_common.gd")\n'
                "\n"
                "static func run(scene_tree: SceneTree) -> Dictionary:\n"
                "	await scene_tree.process_frame\n"
                "	var boot := await _Common.start_new_game(scene_tree)\n"
                "	if not boot.passed:\n"
                '		return {"ok": false, "error": boot.error}\n'
                "\n"
                "	var game: Game = boot.game\n"
                "	var character := game.character\n"
                "	var position_before := character.global_position\n"
                "\n"
                "	scene_tree.paused = true\n"
                "	var elapsed_sec := 0.0\n"
                "	while elapsed_sec < 0.2:\n"
                "		await scene_tree.process_frame\n"
                "		elapsed_sec += scene_tree.get_process_delta_time()\n"
                "\n"
                "	return {\n"
                '		"ok": scene_tree.paused and character.global_position == position_before,\n'
                '		"paused": scene_tree.paused,\n'
                '		"position_unchanged": character.global_position == position_before,\n'
                "	}\n"
                "```\n"
                "\n"
                "### 示例 2：推进游戏时间 1 秒\n"
                "```gdscript\n"
                "extends RefCounted\n"
                "\n"
                'const _Common := preload("res://tests/white_tests/_common.gd")\n'
                "\n"
                "static func run(scene_tree: SceneTree) -> Dictionary:\n"
                "	await scene_tree.process_frame\n"
                "	var boot := await _Common.start_new_game(scene_tree)\n"
                "	if not boot.passed:\n"
                '		return {"ok": false, "error": boot.error}\n'
                "\n"
                "	scene_tree.paused = false\n"
                "	var elapsed_sec := 0.0\n"
                "	while elapsed_sec < 1.0:\n"
                "		await scene_tree.process_frame\n"
                "		elapsed_sec += scene_tree.get_process_delta_time()\n"
                "\n"
                "	return {\n"
                '		"ok": elapsed_sec >= 0.99,\n'
                '		"elapsed_sec": elapsed_sec,\n'
                "	}\n"
                "```\n"
                f"\n## 需求\n{prompt}",
            )
            try:
                tester.tools = [*_common.GIT_READ_ONLY_TOOLS, godot_game_tools.run_white_test]
                submitted = await asyncio.wait_for(
                    tester.request_submit({
                        "is_passed": (bool, "测试是否通过"),
                        "summary": (str, "测试结果摘要"),
                    }),
                    timeout=600.0,
                )
            except asyncio.TimeoutError:
                return False, "白盒测试超时（600s）"
            return submitted["is_passed"], submitted["summary"]
    finally:
        if game_process.poll() is None:
            game_process.kill()
            game_process.wait(timeout=5)


async def begin_develop_workflow(description: str) -> tuple[bool, str]:
    """运行开发工作流：编码、验收、白盒测试循环，直至通过或耗尽重试。"""
    developer = egent.agent.Agent(
        "gpt5-flash",
        skills=_common.discover_project_skills(),
    )
    printer = conversation_printer.ConversationPrinter(developer)
    developer.add_message("system", "你是这个项目的开发工程师")
    developer.add_message(
        "system",
        "你收到了新的需求.请做完这个需求并更新回归测试代码.如果任务无法完成,请说明原因并放弃任务.",
    )

    for _ in range(5):
        try:
            finished, coding_message = await coding(developer, description)
        except CodingGaveUp as error:
            return False, f"你的手下放弃了任务。原因是: \n{error.reason}"

        if not finished:
            developer.add_message("system", "你的工作无法顺利完成。请总结本次工作")
            await printer.request()
            return False, (
                "工作无法顺利完成\n\n"
                + developer.last_message
                + "\n\n---\n回归测试:\n"
                + f"❌ 未通过（已重试 5 次）\n\n{coding_message}"
            )

        passed, accept_message = await workflow_review.review(description)
        if passed:
            test_passed, test_message = await test(description)
            if test_passed:
                developer.add_message(
                    "system",
                    "审查与白盒测试均通过！请总结本次工作。",
                )
                await printer.request()
                return True, (
                    "工作顺利完成\n\n"
                    + developer.last_message
                    + "\n\n---\n验收结果:\n"
                    + f"✅ 验收通过\n\n{accept_message}\n\n"
                    + "---\n白盒测试:\n"
                    + f"✅ 通过\n\n{test_message}\n\n当前状态:等待提交"
                )

            await developer.summarize()
            developer.add_message(
                "system",
                f"白盒测试未通过，请修复:\n\n{test_message}",
            )
            continue

        await developer.summarize()
        developer.add_message(
            "system",
            f"验收未通过，请根据验收意见修复:\n\n{accept_message}",
        )

    developer.add_message("system", "你的工作无法顺利完成。请总结本次工作")
    await printer.request()
    return False, "工作无法顺利完成\n\n" + developer.last_message
