"""egent 工作流共享辅助代码。"""

from __future__ import annotations

import asyncio
from collections.abc import Awaitable, Callable
from pathlib import Path

import egent.builtin_tools.git_tools

_PROJECT_SKILLS_ROOT = Path(".agents/skills")

GIT_READ_ONLY_TOOLS = egent.builtin_tools.git_tools.read_only_tools


def discover_project_skills() -> tuple[Path, ...]:
    """返回项目 ``.agents/skills`` 下所有含 SKILL.md 的技能目录。"""
    return tuple(
        skill_directory
        for skill_directory in sorted(_PROJECT_SKILLS_ROOT.iterdir())
        if skill_directory.is_dir() and (skill_directory / "SKILL.md").is_file()
    )


def run_cli(async_main: Callable[[], Awaitable[int]]) -> None:
    """运行 async_main 并以其返回值作为进程退出码。"""
    raise SystemExit(asyncio.run(async_main()))
