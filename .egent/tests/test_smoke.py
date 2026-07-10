"""egent 工作流冒烟测试。"""

from __future__ import annotations

from pathlib import Path


def test_egent_entry_files_exist() -> None:
    egent_dir = Path(__file__).resolve().parent.parent
    assert (egent_dir / "main.py").is_file()
    assert (egent_dir / "workflow_egent_develop.py").is_file()


def test_common_git_tools_importable() -> None:
    import _common

    assert _common.GIT_READ_ONLY_TOOLS
