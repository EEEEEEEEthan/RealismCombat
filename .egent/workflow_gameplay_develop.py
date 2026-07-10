"""游戏玩法开发工作流：编码、验收、白盒测试循环。"""

from __future__ import annotations

from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator
import workflow_coding
import workflow_review
import workflow_test


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
    project_root = Path.cwd().resolve().as_posix()
    path_permissions = egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
                f"{project_root}/.agents",
                f"{project_root}/.agents/*",
                f"{project_root}/.cursor",
                f"{project_root}/.cursor/*",
                f"{project_root}/.egent",
                f"{project_root}/.egent/*",
                f"{project_root}/.engine",
                f"{project_root}/.engine/*",
                f"{project_root}/.export",
                f"{project_root}/.export/*",
                f"{project_root}/.git",
                f"{project_root}/.git/*",
                f"{project_root}/.godot",
                f"{project_root}/.godot/*",
                f"{project_root}/.logs",
                f"{project_root}/.logs/*",
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
                f"{project_root}/.agents",
                f"{project_root}/.agents/*",
                f"{project_root}/.cursor",
                f"{project_root}/.cursor/*",
                f"{project_root}/.egent",
                f"{project_root}/.egent/*",
                f"{project_root}/.engine",
                f"{project_root}/.engine/*",
                f"{project_root}/.export",
                f"{project_root}/.export/*",
                f"{project_root}/.git",
                f"{project_root}/.git/*",
                f"{project_root}/.godot",
                f"{project_root}/.godot/*",
                f"{project_root}/.logs",
                f"{project_root}/.logs/*",
            ),
        ),
    )

    for _ in range(5):
        try:
            finished, coding_message = await workflow_coding.coding(
                developer,
                description,
                custom_path_permissions=path_permissions,
            )
        except workflow_coding.CodingGaveUp as error:
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
            test_passed, test_message = await workflow_test.test(description)
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
