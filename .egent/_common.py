"""egent 工作流共享辅助代码。"""

from __future__ import annotations

from datetime import datetime
from pathlib import Path

import egent.builtin_tools.git_tools

_PROJECT_ROOT = Path(__file__).resolve().parent
_PROJECT_SKILLS_ROOT = Path(".agents/skills")

GIT_READ_ONLY_TOOLS = egent.builtin_tools.git_tools.read_only_tools


def discover_project_skills() -> tuple[Path, ...]:
    """返回项目 ``.agents/skills`` 下所有含 SKILL.md 的技能目录。"""
    return tuple(
        skill_directory
        for skill_directory in sorted(_PROJECT_SKILLS_ROOT.iterdir())
        if skill_directory.is_dir() and (skill_directory / "SKILL.md").is_file()
    )


def make_fuck(prefix: str) -> callable:
    """创建向 .egent/.fuck.txt 追加吐槽的闭包。

    @param prefix: 前缀标签，如 '[egent开发'
    """
    def fuck(msg: str) -> str:
        """向 .egent/.fuck.txt 追加吐槽，用于收集工作流问题。

        @param msg: 吐槽内容
        """
        fuck_path = _PROJECT_ROOT / ".fuck.txt"
        fuck_path.parent.mkdir(parents=True, exist_ok=True)
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        with open(fuck_path, "a", encoding="utf-8") as _f:
            _f.write(f"{prefix} {timestamp}] {msg}\n")
        return "吐槽已记录。感谢反馈！"

    return fuck
