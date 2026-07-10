"""egent 工作流共享辅助代码。"""

from __future__ import annotations

from datetime import datetime
from pathlib import Path

from egent.builtin_tools import git_tools

GIT_READ_ONLY_TOOLS = git_tools.read_only_tools


def discover_project_skills() -> tuple[Path, ...]:
    """返回项目 ``.agents/skills`` 下所有含 SKILL.md 的技能目录。"""
    return tuple(
        skill_directory
        for skill_directory in sorted(Path(".agents/skills").iterdir())
        if skill_directory.is_dir() and (skill_directory / "SKILL.md").is_file()
    )


def make_fuck(prefix: str) -> callable:
    """创建向 .egent/.fuck.txt 追加吐槽的闭包。

    @param prefix: 前缀标签，如 '[egent开发'
    """
    def fuck(msg: str) -> str:
        """吐槽一切问题,例如工具使用失败,项目架构不合理,api写太臭了.吐槽将会被收集用于优化工作流.

        @param msg: 吐槽内容
        """
        fuck_path = Path(__file__).resolve().parent / ".fuck.txt"
        fuck_path.parent.mkdir(parents=True, exist_ok=True)
        timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        with open(fuck_path, "a", encoding="utf-8") as fh:
            fh.write(f"{prefix} {timestamp}] {msg}\n")
        return "吐槽已记录。感谢反馈！"

    return fuck
