"""egent 工作流共享辅助代码。"""

from __future__ import annotations

import asyncio
from collections.abc import Awaitable, Callable
from pathlib import Path, PurePosixPath
from typing import override

import egent.builtin_tools.file_system_tools
import egent.builtin_tools.git_tools
import egent.builtin_tools.path_validator
import egent.tool

_SENSITIVE_PATTERNS: tuple[str, ...] = (
    "**/.model.toml",
)

_SEARCH_EXCLUDED_PATTERNS: tuple[str, ...] = (
    "**/.model.toml",
    "**/.git",
    "**/*.pyc",
    "**/.pytest_cache",
    "**/.ruff_cache",
    "**/__pycache__",
)

_HIDDEN_DIRECTORY_NAMES: frozenset[str] = frozenset({
    ".agents",
    ".cursor",
    ".egent",
    ".engine",
    ".export",
    ".git",
    ".godot",
    ".logs",
})

_PROJECT_SKILLS_ROOT = Path(".agents/skills")


def discover_project_skills() -> tuple[Path, ...]:
    """返回项目 ``.agents/skills`` 下所有含 SKILL.md 的技能目录。"""
    return tuple(
        skill_directory
        for skill_directory in sorted(_PROJECT_SKILLS_ROOT.iterdir())
        if skill_directory.is_dir() and (skill_directory / "SKILL.md").is_file()
    )


def _matches_path_patterns(relative_text: str, patterns: tuple[str, ...]) -> bool:
    path_segments = PurePosixPath(relative_text).parts
    for segment_count in range(1, len(path_segments) + 1):
        path_prefix = PurePosixPath(*path_segments[:segment_count])
        if any(path_prefix.full_match(pattern) for pattern in patterns):
            return True
    return False


class EgentPathValidator(egent.builtin_tools.path_validator.PathValidator):
    """路径校验：cwd 内可发现，敏感文件禁读写，搜索排除噪声路径。"""

    def __relative_posix(self, path: Path) -> str | None:
        try:
            return path.resolve().relative_to(Path.cwd().resolve()).as_posix()
        except ValueError:
            return None

    def __is_sensitive(self, path: Path) -> bool:
        relative_text = self.__relative_posix(path)
        if relative_text is None:
            return False
        return _matches_path_patterns(relative_text, _SENSITIVE_PATTERNS)

    def __is_search_excluded(self, path: Path) -> bool:
        relative_text = self.__relative_posix(path)
        if relative_text is None:
            return True
        return _matches_path_patterns(relative_text, _SEARCH_EXCLUDED_PATTERNS)

    @override
    def _is_discoverable(self, path: Path) -> bool:
        return self.__relative_posix(path) is not None

    @override
    def _is_readable(self, path: Path) -> bool:
        return self.__relative_posix(path) is not None and not self.__is_sensitive(path)

    @override
    def _is_editable(self, path: Path) -> bool:
        return self.__relative_posix(path) is not None and not self.__is_sensitive(path)

    @override
    def _is_searchable(self, path: Path) -> bool:
        return self.__relative_posix(path) is not None and not self.__is_search_excluded(path)


class HiddenDirectoryPathValidator(EgentPathValidator):
    """隐藏目录不可发现/编辑，但仍可读（read_file 等）。"""

    def _is_in_hidden_directory(self, path: Path) -> bool:
        try:
            relative_parts = path.resolve().relative_to(Path.cwd().resolve()).parts
        except ValueError:
            return False
        return any(part in _HIDDEN_DIRECTORY_NAMES for part in relative_parts)

    @override
    def _is_discoverable(self, path: Path) -> bool:
        return super()._is_discoverable(path) and not self._is_in_hidden_directory(path)

    @override
    def _is_editable(self, path: Path) -> bool:
        return super()._is_editable(path) and not self._is_in_hidden_directory(path)


_DEFAULT_PATH_VALIDATOR = HiddenDirectoryPathValidator()
FILE_READ_TOOLS = egent.builtin_tools.file_system_tools.get_read_tools(_DEFAULT_PATH_VALIDATOR)
FILE_WRITE_TOOLS = egent.builtin_tools.file_system_tools.get_edit_tools(_DEFAULT_PATH_VALIDATOR)
GIT_READ_TOOLS = (*FILE_READ_TOOLS, *egent.builtin_tools.git_tools.read_only_tools)


def run_cli(async_main: Callable[[], Awaitable[int]]) -> None:
    """运行 async_main 并以其返回值作为进程退出码。"""
    raise SystemExit(asyncio.run(async_main()))
