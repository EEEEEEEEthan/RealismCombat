"""游戏玩法开发工作流：编码、验收、白盒测试循环。"""

from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator
import godot_game_tools

_PROJECT_ROOT = Path(__file__).resolve().parent.parent
if str(_PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(_PROJECT_ROOT))

from tests.run_tests import run_regression  # pylint: disable=wrong-import-position


def _make_path_permissions(
    project_root: str,
    *,
    extra_discoverable_blacklist: tuple[str, ...] = (),
    extra_readable_blacklist: tuple[str, ...] = (),
    editable_whitelist: tuple[str, ...] = (),
    editable_blacklist: tuple[str, ...] = (),
) -> egent.builtin_tools.path_validator.PathPermissions:
    """创建项目通用的 PathPermissions，各函数仅覆写差异部分。"""
    _PathRule = egent.builtin_tools.path_validator.PathPermissionRule
    _discoverable_blacklist = (
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
    )
    _readable_blacklist = ("*/.model.toml",)
    return egent.builtin_tools.path_validator.PathPermissions(
        discoverable=_PathRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=_discoverable_blacklist + extra_discoverable_blacklist,
        ),
        readable=_PathRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=_readable_blacklist + extra_readable_blacklist,
        ),
        editable=_PathRule(
            whitelist=editable_whitelist,
            blacklist=editable_blacklist,
        ),
    )


async def review(prompt: str) -> tuple[bool, str]:
    """验收开发成果是否满足需求。"""
    reviewer = egent.agent.Agent(
        "gpt5",
        skills=_common.discover_project_skills(),
    )
    project_root = Path.cwd().resolve().as_posix()
    reviewer.path_permissions = _make_path_permissions(project_root)
    fuck = _common.make_fuck("[gameplay审查")

    with conversation_printer.ConversationPrinter(reviewer, indent=2):
        reviewer.add_message(
            "system",
            "你是这个项目的验收员。你需要验收开发成果是否满足需求。"
            "使用 git_diff 查看代码变更，结合当前项目结构和需求文档进行验收。"
            f"\n\n## 需求:\n{prompt}\n\n"
            "## 验收标准:\n"
            "验证变更是否符合需求\n"
            "验证回归测试是否覆盖了本次修改\n"
            "验证实现是否追求最优雅解，而非最小改动；若仅为凑合可用、补丁堆砌或未做必要重构，应驳回\n"
            "根据 code-optimize 技能检查维护成本与结构质量\n\n"
            "验收通过或者拒绝,都要使用 submit_task 提交验收结果\n\n"
            "遇到任何令你不满的问题（工具失败、架构糟糕、API 设计烂等），请使用 fuck 工具吐槽反馈。\n",
        )
        reviewer.tools = [*_common.GIT_READ_ONLY_TOOLS, fuck]
        submitted = await reviewer.request_submit({
            "is_accepted": (bool, "是否通过验收"),
            "summary": (str, "验收意见摘要"),
        })
    return submitted["is_accepted"], submitted["summary"]


async def coding(
    coder: egent.agent.Agent,
    prompt: str,
) -> tuple[bool, str]:
    """执行开发：实现、优化、跑回归测试；最多重试直至通过。"""
    project_root = Path.cwd().resolve().as_posix()
    coder.path_permissions = _make_path_permissions(
        project_root,
        editable_whitelist=(project_root, f"{project_root}/*"),
        editable_blacklist=(
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
    )

    def _run_batch() -> tuple[bool, str]:
        """运行回归测试批处理，返回 (是否通过, 输出)。"""
        regression_bat = _PROJECT_ROOT / "run-regression.bat"
        timeout_seconds = 120.0
        try:
            test_result = subprocess.run(
                ["cmd", "/c", str(regression_bat)],
                cwd=_PROJECT_ROOT,
                capture_output=True,
                text=True,
                check=False,
                timeout=timeout_seconds,
            )
        except subprocess.TimeoutExpired:
            return False, f"回归测试超时（{timeout_seconds:.0f}s）"
        if test_result.returncode == 0:
            return True, "测试通过"
        return False, f"{test_result.stdout}\n{test_result.stderr}".strip()

    def run_regression_test(spec: str) -> str:
        """运行指定回归测试套件并返回输出。

        @param spec: 测试套件：all（全部）、smoke
        """
        _, output = run_regression(spec)
        if output.startswith("error:"):
            raise RuntimeError(output.removeprefix("error:").strip())
        return output

    fuck = _common.make_fuck("[gameplay开发")

    coder.add_message(
        "system",
        f"{prompt}"
        "## 实现原则\n"
        "不要追求最小 diff，应追求最优雅、最易维护的实现。\n"
        "若现有结构阻碍正确性、可读性或可扩展性，主动重构相关代码；宁可多做一步，也不留补丁式或凑合式修改。"
        "编码完整后使用run_regression_test进行你的专项测试(不需要全跑.跑你相关的专项测试即可.你提交之后会有专门的流程跑测试)。\n\n"
        "遇到任何令你不满的问题（工具失败、架构糟糕、API 设计烂等），请使用 fuck 工具吐槽反馈。",
    )

    last_failure_output = ""
    for _ in range(5):
        coder.tools = [
            *_common.GIT_READ_ONLY_TOOLS,
            run_regression_test,
            godot_game_tools.execute,
            fuck,
        ]
        submitted = await coder.request_submit({
            "success": (bool, "true表示任务完成,false表示放弃"),
            "reason": (str, "如果放弃，填放弃原因,例如需求不合理,或者无法实现等。否则填一个减号`-`"),
        })

        if not submitted["success"]:
            raise RuntimeError(submitted["reason"])

        coder.add_message(
            "system",
            "编码已完成。请使用 code-optimize技能优化代码",
        )
        coder.tools = [*_common.GIT_READ_ONLY_TOOLS, fuck]
        await coder.request()

        passed, last_failure_output = _run_batch()
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


async def test(_prompt: str) -> tuple[bool, str]:
    """通过 execute 白盒校验游戏是否满足需求。"""
    _processes: list[subprocess.Popen] = []

    def launch_game() -> str:
        """启动 Godot 游戏并返回 MCP 端口号与日志路径。"""
        port, proc, log_path = godot_game_tools.launch_game_session()
        _processes.append(proc)
        return json.dumps({"port": port, "log_path": log_path.as_posix()}, ensure_ascii=False, indent=2)

    try:
        tester = egent.agent.Agent("gpt5")
        project_root = Path.cwd().resolve().as_posix()
        tester.path_permissions = _make_path_permissions(
            project_root,
            extra_discoverable_blacklist=(f"{project_root}/tests/regression",),
            extra_readable_blacklist=(
                f"{project_root}/tests/regression",
                f"{project_root}/tests/regression/*",
            ),
            editable_whitelist=(
                f"{project_root}/tests/white_tests",
                f"{project_root}/tests/white_tests/*",
            ),
        )
        with conversation_printer.ConversationPrinter(tester, indent=3):
            tester.add_message(
                "system",
                "你是这个项目的白盒测试员：编写并执行测试脚本，从运行中的游戏实例读取状态，"
                "校验开发成果是否满足需求。"
                "\n\n"
                "## 工作流程\n"
                "0. 调用 `launch_game()` 启动游戏获取端口；从返回的 JSON 解析得到 port 和 log_path。后续步骤复用这个 port 和 log_path。\n"
                "1. walk_files / git_diff 了解变更与场景结构\n"
                "2. 拆用例清单，在 tests/white_tests 下编写测试脚本（每个脚本只做一件简单的事,用来模拟玩家操作或者查看场景树），"
                "并在 tests/white_tests/index.md 登记脚本功能；优先复用已有脚本与 _common.gd\n"
                "3. 使用步骤 0 获取的 port 调用 `execute(port=port, script=..., timeout=...)` 执行测试；"
                "失败查日志（log_path），必要时修正脚本后重跑\n"
                "4. 汇总各用例结果（含期望/实际差异），用 submit_task 提交\n"
                "\n"
                "## 测试脚本目录\n"
                "- 所有白盒测试 .gd 脚本写在 tests/white_tests/ 下（你仅有此目录写权限）\n"
                "- tests/white_tests/index.md 记录每个脚本的功能，鼓励复用、避免重复造轮子\n"
                "- 禁止访问 tests/regression（回归测试由其他流程负责）\n"
                "\n"
                "## execute 脚本约定\n"
                "- script：完整 GDScript 源码，须 extends RefCounted 并定义 static func run(scene_tree: SceneTree) -> void\n"
                "- 用 print() 输出关键值，不要用 return 返回数据\n"
                "- 可 preload res://tests/white_tests/_common.gd 复用 start_new_game / wait_until 等工具\n"
                "- 每个脚本只做一件简单的事；复杂场景拆成多个脚本组合调用\n"
                "\n"
                "### 示例 1：暂停游戏\n"
                "```gdscript\n"
                "extends RefCounted\n"
                "\n"
                "static func run(scene_tree: SceneTree) -> void:\n"
                "	scene_tree.paused = true\n"
                '	print("paused: ", scene_tree.paused)\n'
                "```\n"
                "\n"
                "### 示例 2：推进游戏时间 1 秒\n"
                "```gdscript\n"
                "extends RefCounted\n"
                "\n"
                "static func run(scene_tree: SceneTree) -> void:\n"
                "	var timer := scene_tree.create_timer(1.0, false, false, true)\n"
                "	await timer.timeout\n"
                '	print("1 second passed")\n'
                "```\n\n"
                "遇到任何令你不满的问题（工具失败、架构糟糕、API 设计烂等），请使用 fuck 工具吐槽反馈。\n",
            )
            tester.tools = [
                *_common.GIT_READ_ONLY_TOOLS,
                godot_game_tools.execute,
                launch_game,
            ]
            submitted = await tester.request_submit({
                "success": (bool, "true表示所有用例通过"),
                "summary": (str, "测试摘要"),
            })
        return submitted["success"], submitted["summary"]
    finally:
        for proc in _processes:
            if proc.poll() is None:
                proc.kill()


async def begin_develop_workflow(description: str) -> tuple[bool, str]:
    """运行游戏玩法开发工作流：编码、验收、白盒测试循环，直至通过或耗尽重试。"""
    developer = egent.agent.Agent(
        "gpt5-flash",
        skills=_common.discover_project_skills(),
    )
    printer = conversation_printer.ConversationPrinter(developer, indent=1)
    developer.add_message("system", "你是这个项目的游戏玩法开发工程师")
    developer.add_message(
        "system",
        "你收到了新的需求：\n"
        f"{description}\n\n"
        "如果任务无法完成，请说明原因并放弃任务。\n\n"
        "遇到任何令你不满的问题（工具失败、架构糟糕、API 设计烂等），请使用 fuck 工具吐槽反馈。",
    )

    for _ in range(5):
        try:
            finished, coding_message = await coding(developer, description)
        except RuntimeError as error:
            return False, f"你的手下放弃了任务。原因是: \n{error}"

        if not finished:
            developer.add_message("system", "你的工作无法顺利完成。请总结本次工作")
            await printer.request()
            return False, (
                "工作无法顺利完成\n\n"
                + developer.last_message
                + "\n\n---\n回归测试:\n"
                + f"❌ 未通过（已重试 5 次）\n\n{coding_message}"
            )

        passed, review_message = await review(description)
        if not passed:
            developer.add_message(
                "system",
                f"验收未通过:\n\n{review_message}\n\n请仔细查看需求:\n\n{description}",
            )
            continue

        passed, test_message = await test(description)
        if passed:
            return True, "全部通过"

        developer.add_message(
            "system",
            f"白盒测试未通过:\n\n{test_message}\n\n请仔细查看需求:\n\n{description}",
        )

    return False, "白盒测试连续失败，流程中止"
