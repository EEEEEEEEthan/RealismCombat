"""egent 工作流冒烟测试。"""

from __future__ import annotations

from pathlib import Path

import _common


def test_egent_entry_files_exist() -> None:
    """入口模块文件应存在。"""
    egent_dir = Path(__file__).resolve().parent.parent
    assert (egent_dir / "main.py").is_file()
    assert (egent_dir / "workflow_egent_develop.py").is_file()


def test_common_git_tools_importable() -> None:
    """共享 git 只读工具应可导入。"""
    assert _common.GIT_READ_ONLY_TOOLS
