"""godot_game_tools 参数校验单元测试。"""

from __future__ import annotations

import pytest

import godot_game_tools


def test_run_gdscript_rejects_empty_script() -> None:
    """run_gdscript 在脚本为空时应抛出异常。"""
    with pytest.raises(ValueError, match="脚本源码为空"):
        godot_game_tools.run_gdscript(1, "   ", timeout=1.0)


def test_run_white_test_rejects_outside_directory() -> None:
    """run_white_test 应拒绝 white_tests 目录外的脚本。"""
    with pytest.raises(ValueError, match="脚本路径无效或不存在"):
        godot_game_tools.run_white_test(1, "../outside.gd", timeout=1.0)
