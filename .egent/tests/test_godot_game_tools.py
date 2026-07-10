"""godot_game_tools 参数校验单元测试。"""

from __future__ import annotations

import pytest

import godot_game_tools


def test_run_gdscript_rejects_outside_directory() -> None:
    """run_gdscript 应拒绝 white_tests 目录外的脚本。"""
    with pytest.raises(ValueError, match="脚本路径无效或不存在"):
        godot_game_tools.run_gdscript(1, "../outside.gd", timeout=1.0)
