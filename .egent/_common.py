"""egent 工作流共享辅助代码。"""

from __future__ import annotations

import asyncio
import dataclasses
from collections.abc import Awaitable, Callable
from pathlib import Path

import egent.builtin_tools.git_tools
import egent.builtin_tools.path_validator

_PROJECT_SKILLS_ROOT = Path(".agents/skills")

GIT_READ_ONLY_TOOLS = egent.builtin_tools.git_tools.read_only_tools

# agent 不应触碰的项目根目录下的目录
_PROTECTED_DIRECTORY_NAMES: tuple[str, ...] = (
    ".agents",
    ".cursor",
    ".egent",
    ".engine",
    ".export",
    ".git",
    ".godot",
    ".logs",
)


def create_project_path_permissions() -> egent.builtin_tools.path_validator.PathPermissions:
    """项目路径权限：保护目录不可发现不可编辑，模型配置禁读写。

    模式为 fnmatch 全路径匹配（* 可跨越路径分隔符）。
    """
    cwd = Path.cwd().resolve().as_posix()
    # 项目根目录本身及其下所有路径
    project_whitelist = (cwd, f"{cwd}/*")
    protected_directory_patterns = tuple(
        pattern
        for directory_name in _PROTECTED_DIRECTORY_NAMES
        for pattern in (f"{cwd}/{directory_name}", f"{cwd}/{directory_name}/*")
    )
    return egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=project_whitelist,
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
                *protected_directory_patterns,
            ),
        ),
        readable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=project_whitelist,
            blacklist=("*/.model.toml",),
        ),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=project_whitelist,
            blacklist=(
                "*/.model.toml",
                *protected_directory_patterns,
            ),
        ),
    )


def create_read_only_project_path_permissions() -> egent.builtin_tools.path_validator.PathPermissions:
    """只读项目路径权限：可发现可读同上，全部路径不可编辑。"""
    return dataclasses.replace(
        create_project_path_permissions(),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(),
            blacklist=(),
        ),
    )


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
