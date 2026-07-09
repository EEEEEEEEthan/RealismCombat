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
    """通过 run_gdscript 白盒校验游戏是否满足需求。"""
    try:
        game_port, game_process, game_log_path = godot_game_tools.launch_game_session()
    except RuntimeError as error:
        return False, str(error)

    try:
        tester = egent.agent.Agent("gpt5")
        tester.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
            root=Path.cwd().resolve(),
            discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=("**",),
                blacklist=(
                    "**/*.pyc",
                    "**/.pytest_cache",
                    "**/.ruff_cache",
                    "**/__pycache__",
                    ".agents",
                    ".cursor",
                    ".egent",
                    ".engine",
                    ".export",
                    ".git",
                    ".godot",
                    ".logs",
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
                    ".agents",
                    ".cursor",
                    ".egent",
                    ".engine",
                    ".export",
                    ".git",
                    ".godot",
                    ".logs",
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
                f"2. 拆用例清单，运行时对{game_port}使用run_gdscript进行测试"
                "失败查日志，必要时修正重跑\n"
                "3. 汇总各用例结果（含期望/实际差异），用 submit_task 提交\n"
                "\n"
                "## run_gdscript 脚本约定\n"
                "- script：extends RefCounted，定义 static func run(scene_tree: SceneTree) -> Variant\n"
                '- 返回可 JSON 序列化的断言数据（如 {"ok": true, ...}）\n'
                "- 可 preload res://tests/regression/_common.gd 复用 start_new_game / wait_until\n"
                "\n"
                "### 示例 1：暂停游戏\n"
                "```gdscript\n"
                "extends RefCounted\n"
                "\n"
                'const _Common := preload("res://tests/regression/_common.gd")\n'
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
                'const _Common := preload("res://tests/regression/_common.gd")\n'
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
                tester.tools = [*_common.GIT_READ_ONLY_TOOLS, godot_game_tools.run_gdscript]
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
