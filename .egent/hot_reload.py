"""为本地化的 egent 示例提供热重载。"""

from __future__ import annotations

import importlib
import sys

import _bootstrap  # noqa: F401  # pylint: disable=unused-import  # 必须在 import egent 之前
import egent.agent  # pylint: disable=import-error

_LOCAL_MODULE_NAMES = frozenset(
    {
        "_bootstrap",
        "conversation_printer",
        "hot_reload",
        "shell_tools",
        "workflow",
    },
)


def reload_agent(leader: egent.agent.Agent) -> None:
    """重载 egent 与本地示例模块，并更新 Agent 的类对象。"""
    modules_by_depth = sorted(
        (
            (module_name.count("."), module)
            for module_name, module in sys.modules.items()
            if module_name.startswith("egent.") or module_name in _LOCAL_MODULE_NAMES | {"egent"}
        ),
        key=lambda module_item: module_item[0],
        reverse=True,
    )
    for _, module in modules_by_depth:
        try:
            importlib.reload(module)
        except Exception:  # pylint: disable=broad-exception-caught
            pass
    leader.__class__ = egent.agent.Agent
