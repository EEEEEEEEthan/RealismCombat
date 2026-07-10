"""测试 .egent/main.py 模块加载与模块热重载逻辑。"""

# pylint: disable=protected-access

from __future__ import annotations

import importlib
import sys
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
