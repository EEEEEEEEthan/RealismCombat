"""白盒测试工作流。"""

from __future__ import annotations

import asyncio
from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator
import godot_game_tools


async def test(prompt: str) -> tuple[bool, str]:
    """通过 run_white_test 白盒校验游戏是否满足需求。"""
    try:
        game_port, game_process, game_log_path = godot_game_tools.launch_game_session()
    except RuntimeError as error:
        return False, str(error)

    try:
        tester = egent.agent.Agent("gpt5")
        project_root = Path.cwd().resolve().as_posix()
        white_tests_root = f"{project_root}/tests/white_tests"
        regression_root = f"{project_root}/tests/regression"
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
                    regression_root,
                    f"{regression_root}/*",
                ),
            ),
            readable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(project_root, f"{project_root}/*"),
                blacklist=(
                    "*/.model.toml",
                    regression_root,
                    f"{regression_root}/*",
                ),
            ),
            editable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(white_tests_root, f"{white_tests_root}/*"),
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
