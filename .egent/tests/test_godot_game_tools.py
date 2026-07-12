"""godot_game_tools 参数校验单元测试。"""

from __future__ import annotations

import pytest

import godot_game_tools


def test_execute_rejects_non_positive_timeout() -> None:
    """execute 应拒绝 timeout <= 0。"""
    with pytest.raises(ValueError, match="timeout 必须大于 0"):
        godot_game_tools.execute(1, "some script", timeout=0)
    with pytest.raises(ValueError, match="timeout 必须大于 0"):
        godot_game_tools.execute(1, "some script", timeout=-1.0)
