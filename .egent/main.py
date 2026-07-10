"""RealismCombat egent 开发工作流入口。

运行前请在项目根目录配置 ``.egent/.model.toml``::

    pip install -e ".[dev]"
    python .egent/main.py
"""

from __future__ import annotations

import importlib
import sys
from pathlib import Path

_EGENT_DIR = Path(__file__).resolve().parent
if str(_EGENT_DIR) not in sys.path:
    sys.path.insert(0, str(_EGENT_DIR))

# pylint: disable=wrong-import-position
import _common
import conversation_printer
import egent
import egent.agent
import egent.builtin_tools.path_validator
import workflow_egent_develop
import workflow_gameplay_develop

_PROJECT_ROOT = Path(__file__).resolve().parent.parent


def make_delegate_develop_workflow() -> egent.tool.ToolCallable:
    """生成可供 agent 调用的委派工具。"""

    async def delegate_develop_workflow(description: str) -> str:
        """委派开发工作：编码、验收、白盒测试循环，直至通过或耗尽重试。

        @param description 开发需求描述
        """
        success, summary = await workflow_gameplay_develop.begin_develop_workflow(description)
        if success:
            return summary
        repo = str(_PROJECT_ROOT)
        reset_result = egent.builtin_tools.git_tools.git_reset(hard=True, path=repo)
        clean_result = egent.builtin_tools.git_tools.git_clean(path=repo)
        return (
            f"{summary}\n\n---\n工作区已自动清理（git reset --hard + git clean -fd）:\n"
            f"{reset_result}\n{clean_result}"
        )

    return delegate_develop_workflow


def make_delegate_egent_develop_workflow() -> egent.tool.ToolCallable:
    """生成可供 agent 调用的 egent 开发委派工具。"""

    _module_reload_order = (
        "workflow_gameplay_develop",
        "workflow_egent_develop",
        "conversation_printer",
        "egent",
        "egent.agent",
        "egent.builtin_tools.path_validator",
        "_common",
    )

    def _reload_modules() -> None:
        """reload 已在 sys.modules 中的 .egent 模块，失败时只警告不崩溃。"""
        for name in _module_reload_order:
            module = sys.modules.get(name)
            if module is None:
                continue
            try:
                importlib.reload(module)
            except Exception as exc:  # pylint: disable=broad-exception-caught
                print(f"Warning: failed to reload module '{name}': {exc}", file=sys.stderr)

        for key in ("main", "__main__"):
            module = sys.modules.get(key)
            if module is not None:
                try:
                    importlib.reload(module)
                except Exception as exc:  # pylint: disable=broad-exception-caught
                    print(f"Warning: failed to reload module '{key}': {exc}", file=sys.stderr)
                break

    async def delegate_egent_develop_workflow(description: str) -> str:
        """委派 egent 开发工作：编码、验收、pytest 回归循环，直至通过或耗尽重试。

        @param description 开发需求描述（作用于 .egent 目录）
        """
        success, summary = await workflow_egent_develop.begin_egent_develop_workflow(description)
        if success:
            _reload_modules()
            return summary
        repo = str(_PROJECT_ROOT)
        reset_result = egent.builtin_tools.git_tools.git_reset(hard=True, path=repo)
        clean_result = egent.builtin_tools.git_tools.git_clean(path=repo)
        return (
            f"{summary}\n\n---\n工作区已自动清理（git reset --hard + git clean -fd）:\n"
            f"{reset_result}\n{clean_result}"
        )

    return delegate_egent_develop_workflow


async def run_turn(
    agent: egent.agent.Agent,
    printer: conversation_printer.ConversationPrinter,
) -> None:
    """运行一轮交互：收集用户输入并发送请求。"""
    prompt = input(">>> ").strip()
    agent.add_message("user", prompt)
    await printer.request(
        tools=[
            *_common.GIT_READ_ONLY_TOOLS,
            make_delegate_develop_workflow(),
            make_delegate_egent_develop_workflow(),
            egent.builtin_tools.git_tools.git_add,
            egent.builtin_tools.git_tools.git_commit,
            egent.builtin_tools.git_tools.git_push,
        ],
    )


async def async_main() -> int:
    """运行交互式聊天，返回进程退出码。"""
    agent = egent.agent.Agent("gpt5", skills=_common.discover_project_skills())
    project_root = Path.cwd().resolve().as_posix()
    agent.path_permissions = egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
            ),
        ),
        readable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=("**",),
            blacklist=("*/.model.toml",),
        ),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(),
            blacklist=(),
        ),
    )
    agent.add_message(
        "system",
        "你是egent.你是这个游戏项目的主程\n"
        "和你对接的人是制作人.你可能需要根据项目的实际情况揣测他背后的真实需求.你需要整理一份大致的计划.计划不要超过20行,每行不要超过160字.\n"
        "在制作人明确表达让你开始执行之前,不要执行.\n"
        "执行过程你需要尽可能分步骤委派任务,每个任务尽可能小,独立,可验收.任务提交后要阅读报告.\n"
        "游戏玩法相关用 delegate_develop_workflow；egent 工作流/工具相关用 delegate_egent_develop_workflow。\n"
        "关于每一个任务:\n"
        "如果任务成功,你应该阅读任务报告,和gitdiff,分析是否满足你的要求.如果满足,你可以gitcommit并委派下一个任务.你应该commit所有修改,不要遗漏.\n"
        "如果任务失败,你需要分析为什么失败,调整任务描述后重新委派.失败时工作区会自动清理,报告末尾会说明.\n"
        "当然需求本身可能不合理.如果遇到这种情况,你认为调整任务描述也无法完成,那你就应该立即终止并且将原因反馈给我.\n"
    )
    printer = conversation_printer.ConversationPrinter(agent)
    while True:
        await run_turn(agent, printer)


def run() -> None:
    """CLI 入口。"""
    _common.run_cli(async_main)


if __name__ == "__main__":
    run()
