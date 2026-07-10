"""信息采集 workflow：纯只读 Agent 分析项目信息并提交报告。"""

from __future__ import annotations

from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator


async def begin_info_collect_workflow(description: str) -> tuple[bool, str]:
    """运行信息采集 workflow：纯只读 Agent 分析项目，提交分析报告。

    Agent 使用 submit_task 提交 ``{"success": bool, "summary": str}`` 报告。
    工作区为只读，不会产生任何脏写。
    """
    collector = egent.agent.Agent(
        "gpt5-flash",
        skills=_common.discover_project_skills(),
    )

    project_root = Path.cwd().resolve().as_posix()
    collector.path_permissions = (
        egent.builtin_tools.path_validator.PathPermissions(
            discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(project_root, f"{project_root}/*"),
                blacklist=(
                    "*.pyc",
                    "*/.pytest_cache",
                    "*/.ruff_cache",
                    "*/__pycache__",
                    f"{project_root}/.git",
                    f"{project_root}/.godot",
                    f"{project_root}/.export",
                    f"{project_root}/.logs",
                ),
            ),
            readable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(project_root, f"{project_root}/*"),
                blacklist=("*/.model.toml",),
            ),
            editable=egent.builtin_tools.path_validator.PathPermissionRule(
                whitelist=(),
                blacklist=(),
            ),
        )
    )

    fuck = _common.make_fuck("[信息采集")

    with conversation_printer.ConversationPrinter(collector, indent=1):
        collector.add_message(
            "system",
            "你是这个项目的信息采集员。请根据需求分析项目代码结构、配置等信息。\n\n"
            f"## 需求:\n{description}\n\n"
            "请进行信息采集和分析，完成后使用 submit_task 提交分析报告。\n\n"
            "遇到任何令你不满的问题（工具失败、架构糟糕、API 设计烂等），请使用 fuck 工具吐槽反馈。\n",
        )
        collector.tools = [
            *_common.GIT_READ_ONLY_TOOLS,
            fuck,
        ]
        submitted = await collector.request_submit({
            "success": (bool, "true 表示采集成功, false 表示失败"),
            "summary": (str, "采集到的信息摘要和分析报告"),
        })
    return submitted["success"], submitted["summary"]
