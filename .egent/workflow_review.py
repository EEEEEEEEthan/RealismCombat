"""开发成果验收工作流。"""

from __future__ import annotations

from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator


async def review(prompt: str) -> tuple[bool, str]:
    """验收开发成果是否满足需求。"""
    reviewer = egent.agent.Agent(
        "gpt5",
        skills=_common.discover_project_skills(),
    )
    project_root = Path.cwd().resolve().as_posix()
    reviewer.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
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
            whitelist=(),
            blacklist=(),
        ),
    )
    with conversation_printer.ConversationPrinter(reviewer):
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
            "验收通过或者拒绝,都要使用 submit_task 提交验收结果\n",
        )
        reviewer.tools = list(_common.GIT_READ_ONLY_TOOLS)
        submitted = await reviewer.request_submit({
            "is_accepted": (bool, "是否通过验收"),
            "summary": (str, "验收意见摘要"),
        })
    return submitted["is_accepted"], submitted["summary"]
