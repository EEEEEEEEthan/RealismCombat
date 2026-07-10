"""将 Agent 流式事件打印到终端。"""

from __future__ import annotations

import json
from collections.abc import Iterable
from types import TracebackType

import egent.agent
import egent.tool


def _truncate(text: str, max_chars: int) -> str:
    """如果 text 超过 max_chars 则截断并附加 ``...``。"""
    if len(text) <= max_chars:
        return text
    return text[:max_chars] + "..."


def _format_arguments(arguments_json: str) -> str:
    """将 JSON 参数字符串格式化为 ``key=val, ...`` 形式。

    总额度约 ``PARAM_BUDGET`` 字符（不含两端空格），多参数平分，超出部分截断。
    """
    try:
        args = json.loads(arguments_json)
    except (json.JSONDecodeError, TypeError):
        return arguments_json

    if not isinstance(args, dict):
        return str(args)

    parts: list[str] = []
    n = len(args)
    if n == 0:
        return ""

    per_budget = max(120 // n, 10)

    for key in args:
        value = args[key]
        value_str = str(value)
        prefix_len = len(key) + 1  # "key="
        val_budget = max(per_budget - prefix_len, 3)
        truncated_val = _truncate(value_str, val_budget)
        parts.append(f"{key}={truncated_val}")

    return ", ".join(parts)


def _first_line_and_has_more(text: str) -> tuple[str, bool]:
    """返回 (第一个非空行, 是否存在第二个非空行)。"""
    first = ""
    found_first = False
    for line in text.splitlines():
        stripped = line.strip()
        if stripped:
            if not found_first:
                first = stripped
                found_first = True
            else:
                return first, True
    return first, False


class ConversationPrinter:
    """监听 Agent 事件并打印到终端。"""

    def __init__(self, agent: egent.agent.Agent) -> None:
        self._agent = agent
        agent.add_listener(self.__handle_event)

    def close(self) -> None:
        """取消事件监听。"""
        self._agent.remove_listener(self.__handle_event)

    def __enter__(self) -> ConversationPrinter:
        return self

    def __exit__(
        self,
        exc_type: type[BaseException] | None,
        exc_value: BaseException | None,
        traceback: TracebackType | None,
    ) -> None:
        self.close()

    async def request(
        self,
        *,
        tools: Iterable[egent.tool.ToolCallable] = (),
    ) -> None:
        """执行一轮请求并打印流式输出。"""
        self._agent.tools = list(tools)
        await self._agent.request()

    def __handle_event(self, event: egent.agent.AgentEvent) -> None:
        if isinstance(event, egent.agent.TextDelta):
            print(event.text, end="", flush=True)
        elif isinstance(event, egent.agent.ToolCallStarted):
            formatted = _format_arguments(event.arguments)
            if formatted:
                print(f"\n[tool_call: {event.name}({formatted})]", flush=True)
            else:
                print(f"\n[tool_call: {event.name}]", flush=True)
        elif isinstance(event, egent.agent.ToolCallExecuted):
            first_line, has_more = _first_line_and_has_more(event.result)
            if first_line:
                suffix = "..." if has_more else ""
                print(f"  => {_truncate(first_line, 200)}{suffix}", flush=True)
        elif isinstance(event, egent.agent.TurnCompleted):
            print(flush=True)
