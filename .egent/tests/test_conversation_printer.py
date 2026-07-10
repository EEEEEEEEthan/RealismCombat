"""Tests for conversation_printer.py."""

from __future__ import annotations

import json
from unittest.mock import MagicMock

import pytest

import egent.agent
from conversation_printer import (
    ConversationPrinter,
    _first_line_and_has_more,
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


class TestFirstLineAndHasMore:
    """Tests for _first_line_and_has_more helper."""

    # --- first line ---

    def test_single_line_first(self) -> None:
        """Single line should return the line and has_more=False."""
        first, has_more = _first_line_and_has_more("hello")
        assert first == "hello"
        assert has_more is False

    def test_multiline_first(self) -> None:
        """First line of multiline text should be returned."""
        first, has_more = _first_line_and_has_more("line1\nline2\nline3")
        assert first == "line1"
        assert has_more is True

    def test_leading_blank_lines_first(self) -> None:
        """Leading blank lines should be skipped."""
        first, has_more = _first_line_and_has_more("\n\n  \ncontent")
        assert first == "content"
        assert has_more is False

    def test_all_blank_first(self) -> None:
        """All-blank input should return empty string and has_more=False."""
        first, has_more = _first_line_and_has_more("  \n\n  ")
        assert first == ""
        assert has_more is False

    def test_empty_string_first(self) -> None:
        """Empty string should return empty string and has_more=False."""
        first, has_more = _first_line_and_has_more("")
        assert first == ""
        assert has_more is False

    # --- has_more ---

    def test_multiple_lines_has_more(self) -> None:
        """Multiple non-empty lines should return has_more=True."""
        _, has_more = _first_line_and_has_more("line1\nline2")
        assert has_more is True

    def test_with_blank_lines_has_more(self) -> None:
        """Blank lines between non-empty lines should still detect more."""
        _, has_more = _first_line_and_has_more("line1\n\n\nline2")
        assert has_more is True

    def test_only_blank_after_first_has_more(self) -> None:
        """Blank lines after first non-empty should return has_more=False."""
        _, has_more = _first_line_and_has_more("content\n  \n  ")
        assert has_more is False

    def test_trailing_newline_single_line_has_more(self) -> None:
        """Single line with trailing newline should return has_more=False."""
        _, has_more = _first_line_and_has_more("hello\n")
        assert has_more is False


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
        """ToolCallExecuted should print first content line with ... if more lines."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="get_weather",
            arguments="{}",
            result="Sunny, 25\u00b0C\nMore details here",
        ))
        captured = capsys.readouterr()
        assert captured.out == "=> Sunny, 25\u00b0C...\n"

        printer.close()

    def test_handle_tool_call_executed_single_line(self, mock_agent, capsys):
        """ToolCallExecuted with single line result should NOT add ..."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="get_weather",
            arguments="{}",
            result="Sunny, 25\u00b0C",
        ))
        captured = capsys.readouterr()
        assert captured.out == "=> Sunny, 25\u00b0C\n"

        printer.close()

    def test_handle_tool_call_executed_blank_then_line(self, mock_agent, capsys):
        """Leading blank lines before second non-empty line should still add ..."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="test",
            arguments="{}",
            result="first\n\n  \nsecond",
        ))
        captured = capsys.readouterr()
        assert captured.out == "=> first...\n"

        printer.close()

    def test_handle_tool_call_executed_trailing_newline(self, mock_agent, capsys):
        """Trailing newline without a second non-empty line should NOT add ..."""
        printer = ConversationPrinter(mock_agent)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="test",
            arguments="{}",
            result="only line\n",
        ))
        captured = capsys.readouterr()
        assert captured.out == "=> only line\n"

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
        assert output.startswith("=> ")
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
        assert "=> Sunny, 25\u00b0C" in output
        assert output.endswith("\n")

        printer.close()


class TestConversationPrinterIndent:
    """Tests for ConversationPrinter with indent > 0."""

    @pytest.fixture
    def mock_agent(self):
        """Create a mock agent with listener support."""
        agent = MagicMock()
        agent.tools = []
        return agent

    def test_indent_zero_default(self, mock_agent, capsys):
        """Default indent=0 should produce no extra spacing."""
        printer = ConversationPrinter(mock_agent, indent=0)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Hello"))
        captured = capsys.readouterr()
        assert captured.out == "Hello"

        printer.close()

    def test_indent_one_text_delta(self, mock_agent, capsys):
        """indent=1 should prefix first TextDelta with 4 spaces."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Hello"))
        captured = capsys.readouterr()
        assert captured.out == "    Hello"

        printer.close()

    def test_indent_one_text_delta_multiple(self, mock_agent, capsys):
        """Only the first TextDelta gets indent prefix; subsequent ones don't."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Hello "))
        handler(egent.agent.TextDelta(text="World"))
        captured = capsys.readouterr()
        assert captured.out == "    Hello World"

        printer.close()

    def test_indent_two_text_delta(self, mock_agent, capsys):
        """indent=2 should prefix first TextDelta with 8 spaces."""
        printer = ConversationPrinter(mock_agent, indent=2)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Hi"))
        captured = capsys.readouterr()
        assert captured.out == "        Hi"

        printer.close()

    def test_indent_one_tool_call_started(self, mock_agent, capsys):
        """ToolCallStarted should use indent prefix after newline."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallStarted(
            name="test_func",
            arguments=json.dumps({"arg": "val"}),
        ))
        captured = capsys.readouterr()
        assert captured.out == "\n    [tool_call: test_func(arg=val)]\n"

        printer.close()

    def test_indent_one_tool_call_started_no_args(self, mock_agent, capsys):
        """ToolCallStarted without args should use indent prefix."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallStarted(
            name="list_files",
            arguments="{}",
        ))
        captured = capsys.readouterr()
        assert captured.out == "\n    [tool_call: list_files]\n"

        printer.close()

    def test_indent_one_tool_call_executed(self, mock_agent, capsys):
        """ToolCallExecuted should use indent prefix instead of hardcoded spaces."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.ToolCallExecuted(
            name="test",
            arguments="{}",
            result="Result line",
        ))
        captured = capsys.readouterr()
        assert captured.out == "    => Result line\n"

        printer.close()

    def test_indent_one_turn_completed_resets_indent(self, mock_agent, capsys):
        """TurnCompleted should reset _indent_printed so next turn gets indent."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        # First turn
        handler(egent.agent.TextDelta(text="First turn. "))
        handler(egent.agent.TurnCompleted(text=""))
        captured = capsys.readouterr()
        assert captured.out == "    First turn. \n"

        # Second turn - should get indent again
        handler(egent.agent.TextDelta(text="Second turn."))
        captured = capsys.readouterr()
        assert captured.out == "    Second turn."

        printer.close()

    def test_indent_one_full_flow(self, mock_agent, capsys):
        """Full flow with indent=1 should indent all elements correctly."""
        printer = ConversationPrinter(mock_agent, indent=1)
        handler = mock_agent.add_listener.call_args[0][0]

        handler(egent.agent.TextDelta(text="Let me "))
        handler(egent.agent.TextDelta(text="check."))
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
        assert output.startswith("    Let me check.")
        assert "\n    [tool_call: get_weather(city=Beijing)]" in output
        assert "\n    => Sunny, 25\u00b0C" in output
        assert output.endswith("\n")

        printer.close()
