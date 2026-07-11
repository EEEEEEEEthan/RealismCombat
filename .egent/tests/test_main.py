"""测试 .egent/main.py 模块加载与模块热重载逻辑。"""

# pylint: disable=protected-access

from __future__ import annotations

import importlib
import inspect
import sys
from pathlib import Path
from unittest.mock import patch

import pytest


def test_importlib_is_imported() -> None:
    """main.py 应已导入 importlib 模块。"""
    import main  # pylint: disable=import-outside-toplevel

    assert hasattr(main, "importlib")


@pytest.mark.asyncio
async def test_reload_modules_called_on_success() -> None:
    """delegate_egent_develop_workflow 在 success=True 时应 reload 已导入的 .egent 模块。"""
    import main  # pylint: disable=import-outside-toplevel
    import workflow_egent_develop  # pylint: disable=import-outside-toplevel

    with (
        patch.object(workflow_egent_develop, "begin_egent_develop_workflow", return_value=(True, "ok")),
        patch.object(importlib, "reload") as mock_reload,
    ):
        delegate = main.make_delegate_egent_develop_workflow()
        result = await delegate("test task")
        assert result == "ok"
        # 应至少 reload 了 _common 模块（如果已加载）
        if "_common" in sys.modules:
            mock_reload.assert_any_call(sys.modules["_common"])
        # main/__main__ 应被 reload
        main_module = sys.modules.get("main") or sys.modules.get("__main__")
        if main_module is not None:
            mock_reload.assert_any_call(main_module)


@pytest.mark.asyncio
async def test_reload_modules_not_called_on_failure() -> None:
    """delegate_egent_develop_workflow 在 success=False 时不应调用 importlib.reload。"""
    import main  # pylint: disable=import-outside-toplevel
    import workflow_egent_develop  # pylint: disable=import-outside-toplevel

    with (
        patch.object(workflow_egent_develop, "begin_egent_develop_workflow", return_value=(False, "fail")),
        patch.object(importlib, "reload") as mock_reload,
        patch("egent.builtin_tools.git_tools.git_reset", return_value=""),
        patch("egent.builtin_tools.git_tools.git_clean", return_value=""),
    ):
        delegate = main.make_delegate_egent_develop_workflow()
        result = await delegate("test task")
        assert "fail" in result
        mock_reload.assert_not_called()


@pytest.mark.asyncio
async def test_reload_modules_handles_failure_gracefully(capsys: pytest.CaptureFixture[str]) -> None:
    """reload 失败时应打印警告但不中断流程。"""
    import main  # pylint: disable=import-outside-toplevel
    import workflow_egent_develop  # pylint: disable=import-outside-toplevel

    def _raise(*_args: object, **_kwargs: object) -> None:
        raise RuntimeError("模拟 reload 失败")

    with (
        patch.object(workflow_egent_develop, "begin_egent_develop_workflow", return_value=(True, "ok")),
        patch.object(importlib, "reload", side_effect=_raise),
    ):
        delegate = main.make_delegate_egent_develop_workflow()
        result = await delegate("test task")
        assert result == "ok"

    captured = capsys.readouterr()
    assert "Warning" in captured.err


@pytest.mark.asyncio
async def test_reload_modules_skips_missing_modules() -> None:
    """不在 sys.modules 中的模块名应被跳过。"""
    import main  # pylint: disable=import-outside-toplevel
    import workflow_egent_develop  # pylint: disable=import-outside-toplevel

    with (
        patch.object(workflow_egent_develop, "begin_egent_develop_workflow", return_value=(True, "ok")),
        patch.object(importlib, "reload") as mock_reload,
    ):
        delegate = main.make_delegate_egent_develop_workflow()
        result = await delegate("test task")
        assert result == "ok"
        # 只应该 reload 在 sys.modules 中已存在的模块
        for call in mock_reload.call_args_list:
            module = call.args[0]
            assert module.__name__ in sys.modules, (
                f"reload 了不在 sys.modules 中的模块: {module.__name__}"
            )


@pytest.mark.asyncio
async def test_run_turn_recovers_from_connection_error(capsys: pytest.CaptureFixture[str]) -> None:
    """run_turn 在 API 连接失败时应回滚消息并继续，而非崩溃。"""
    import main  # pylint: disable=import-outside-toplevel
    from openai import APIConnectionError

    agent = main.egent.agent.Agent("gpt5", skills=())
    agent.add_message("system", "test")
    message_count_before_turn = main._agent_message_count(agent)

    async def fake_request_raises(*, tools: object, **_kwargs: object) -> None:
        _ = tools
        raise APIConnectionError(request=None)

    with (
        patch("builtins.input", return_value="用户输入"),
        patch.object(main.conversation_printer.ConversationPrinter, "request", side_effect=fake_request_raises),
    ):
        await main.run_turn(agent, main.conversation_printer.ConversationPrinter(agent))

    assert main._agent_message_count(agent) == message_count_before_turn
    captured = capsys.readouterr()
    assert "连接失败，请重试" in captured.err


@pytest.mark.asyncio
async def test_run_turn_includes_fuck_tool() -> None:
    """run_turn 的 tools 列表应包含 _common.make_fuck("[主程]") 创建的吐槽工具。"""
    import main  # pylint: disable=import-outside-toplevel

    captured_tools: list | None = None

    async def fake_request(*, tools: object, **_kwargs: object) -> None:
        nonlocal captured_tools
        captured_tools = list(tools)  # type: ignore[arg-type]

    with (
        patch("builtins.input", return_value="test prompt"),
        patch.object(main.conversation_printer.ConversationPrinter, "request", side_effect=fake_request),
    ):
        agent = main.egent.agent.Agent("gpt5", skills=())
        printer = main.conversation_printer.ConversationPrinter(agent)
        await main.run_turn(agent, printer)

    assert captured_tools is not None, "printer.request 未被调用"

    # 验证存在一个可调用的吐槽工具（make_fuck("[主程]") 返回的闭包）
    fuck_tools = [t for t in captured_tools if hasattr(t, "__name__") and t.__name__ == "fuck"]  # pylint: disable=not-an-iterable
    assert len(fuck_tools) == 1, f"期望恰好一个名为 'fuck' 的工具，实际找到 {len(fuck_tools)} 个"

    # 验证该工具确实能写入反馈
    result = fuck_tools[0]("test反馈消息")
    assert "反馈已记录" in result

    # 清理测试写入的反馈
    fuck_path = Path(__file__).resolve().parent.parent / ".fuck.txt"
    if fuck_path.exists():
        content = fuck_path.read_text(encoding="utf-8")
        remaining = [l for l in content.splitlines(keepends=True) if "test反馈消息" not in l]
        fuck_path.write_text("".join(remaining), encoding="utf-8")


def test_async_main_system_prompt_includes_improvement_feedback_phrasing() -> None:
    """async_main 的 system prompt 应包含新版改进反馈措辞。

    验证关键词：优化信号、工具链、运行环境、不要沉默绕行。
    """
    import main  # pylint: disable=import-outside-toplevel

    source = inspect.getsource(main.async_main)
    assert "fuck" in source, "async_main 源码中应包含 'fuck' 相关代码"
    assert "提交改进反馈" in source, "async_main 源码中应包含 '提交改进反馈'"
    assert "优化信号" in source, "async_main 源码中应包含 '优化信号'"
    assert "工具链" in source, "async_main 源码中应包含 '工具链'"
    assert "运行环境" in source, "async_main 源码中应包含 '运行环境'"
    assert "不要沉默绕行" in source, "async_main 源码中应包含 '不要沉默绕行'"
    assert "沉默等于放弃改善的机会" in source, (
        "async_main 源码中应包含 '沉默等于放弃改善的机会'"
    )
