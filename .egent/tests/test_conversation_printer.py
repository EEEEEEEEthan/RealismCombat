"""Tests for conversation_printer.py."""

from __future__ import annotations

import json
from unittest.mock import MagicMock

import pytest

import egent.agent
from conversation_printer import (
    ConversationPrinter,
    _first_content_line,
    _format_arguments,
    _truncate,
)


class TestTruncate:
    """Tests for _truncate helper."""

    def test_short_text(self) -> None:
        """Short text should remain unchanged."""
        assert _truncate("hello", 10) == "hello"

    def test_exact_length(self) -> None:
        """Text at exact max_chars should remain unchanged."""
        assert _truncate("hello", 5) == "hello"

    def test_truncated(self) -> None:
        """Text exceeding max_chars should be truncated with ..."""
        assert _truncate("hello world", 5) == "hello..."

    def test_empty(self) -> None:
        """Empty string should remain empty."""
        assert _truncate("", 10) == ""


class TestFirstContentLine:
    """Tests for _first_content_line helper."""

    def test_single_line(self) -> None:
        """Single line should be returned as-is."""
        assert _first_content_line("hello") == "hello"

    def test_multiline(self) -> None:
        """First line of multiline text should be returned."""
        assert _first_content_line("line1\nline2\nline3") == "line1"

    def test_leading_blank_lines(self) -> None:
        """Leading blank lines should be skipped."""
        assert _first_content_line("\n\n  \ncontent") == "content"

    def test_all_blank(self) -> None:
        """All-blank input should return empty string."""
        assert _first_content_line("  \n\n  ") == ""

    def test_empty_string(self) -> None:
        """Empty string should return empty string."""
        assert _first_content_line("") == ""


class TestFormatArguments:
    """Tests for _format_arguments helper."""

    def test_single_key_value(self) -> None:
        """Single key-value pair should format as key=value."""
        result = _format_arguments(json.dumps({"name": "Alice"}))
        assert result == "name=Alice"

    def test_multiple_keys(self) -> None:
        """Multiple keys should be comma-separated."""
        result = _format_arguments(json.dumps({"a": "1", "b": "2"}))
        assert "a=1" in result
        assert "b=2" in result
        assert ", " in result

    def test_long_value_truncated(self) -> None:
        """Long values should be truncated."""
        long_val = "x" * 200
        result = _format_arguments(json.dumps({"key": long_val}))
        assert len(result) < len(long_val) + 10
        assert result.endswith("...")

    def test_empty_dict(self) -> None:
        """Empty dict should return empty string."""
        assert _format_arguments("{}") == ""

    def test_non_dict_json(self) -> None:
        """Non-dict JSON should be converted to string."""
        assert _format_arguments('["a", "b"]') == "['a', 'b']"

    def test_invalid_json(self) -> None:
        """Invalid JSON should be returned as-is."""
        assert _format_arguments("not json") == "not json"

    def test_numeric_value(self) -> None:
        """Numeric values should be formatted."""
        result = _format_arguments(json.dumps({"count": 42}))
        assert result == "count=42"

    def test_boolean_value(self) -> None:
        """Boolean values should be formatted."""
        result = _format_arguments(json.dumps({"flag": True}))
        assert result == "flag=True"

    def test_none_value(self) -> None:
        """None values should be formatted."""
        result = _format_arguments(json.dumps({"data": None}))
        assert result == "data=None"


class TestConversationPrinterIntegration:
    """Integration-style tests for ConversationPrinter event printing."""

    @pytest.fixture
    def mock_agent(self):
        """Create a mock agent with listener support."""
        agent = MagicMock()
        agent.tools = []
        return agent

    def test_handle_text_delta(self, mock_agent, capsys):
        """TextDelta should print text without newline."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Hello"))
        captured = capsys.readouterr()
        assert captured.out == "Hello"

        printer.close()

    def test_handle_tool_call_started_with_args(self, mock_agent, capsys):
        """ToolCallStarted with arguments should print formatted args."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallStarted(
            name="get_weather",
            arguments=json.dumps({"city": "Beijing", "units": "metric"}),
        ))
        captured = capsys.readouterr()
        output = captured.out
        assert output.startswith("\n[tool_call: get_weather(")
        assert "city=Beijing" in output
        assert "units=metric" in output
        assert output.rstrip().endswith(")]")

        printer.close()

    def test_handle_tool_call_started_no_args(self, mock_agent, capsys):
        """ToolCallStarted without arguments should print just name."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallStarted(
            name="list_files",
            arguments="{}",
        ))
        captured = capsys.readouterr()
        assert captured.out == "\n[tool_call: list_files]\n"

        printer.close()

    def test_handle_tool_call_executed(self, mock_agent, capsys):
        """ToolCallExecuted should print first content line of result."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="get_weather",
            arguments="{}",
            result="Sunny, 25\u00b0C\nMore details here",
        ))
        captured = capsys.readouterr()
        assert captured.out == "  => Sunny, 25\u00b0C\n"

        printer.close()

    def test_handle_tool_call_executed_long_result(self, mock_agent, capsys):
        """ToolCallExecuted with long result should truncate."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        long_line = "A" * 300
        handler(egent.agent.ToolCallExecuted(
            name="test",
            arguments="{}",
            result=long_line,
        ))
        captured = capsys.readouterr()
        output = captured.out
        assert output.startswith("  => ")
        assert output.rstrip().endswith("...")

        printer.close()

    def test_handle_tool_call_executed_empty_result(self, mock_agent, capsys):
        """ToolCallExecuted with empty result should print nothing."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="test",
            arguments="{}",
            result="",
        ))
        captured = capsys.readouterr()
        assert captured.out == ""

        printer.close()

    def test_handle_turn_completed(self, mock_agent, capsys):
        """TurnCompleted should flush with newline."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TurnCompleted(text=""))
        captured = capsys.readouterr()
        assert captured.out == "\n"

        printer.close()

    def test_full_flow(self, mock_agent, capsys):
        """Simulate a full conversation flow."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Let me "))
        handler(egent.agent.TextDelta(text="check the weather."))
        handler(egent.agent.ToolCallStarted(
            name="get_weather",
            arguments=json.dumps({"city": "Beijing"}),
        ))
        handler(egent.agent.ToolCallExecuted(
            name="get_weather",
            arguments=json.dumps({"city": "Beijing"}),
            result="Sunny, 25\u00b0C",
        ))
        handler(egent.agent.TurnCompleted(text="Done"))

        captured = capsys.readouterr()
        output = captured.out
        assert "Let me check the weather." in output
        assert "[tool_call: get_weather(city=Beijing)]" in output
        assert "  => Sunny, 25\u00b0C" in output
        assert output.endswith("\n")

        printer.close()
