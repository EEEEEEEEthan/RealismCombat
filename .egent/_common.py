"""egent 工作流共享辅助代码。"""

from __future__ import annotations

from datetime import datetime
from pathlib import Path

from egent.builtin_tools import git_tools

GIT_READ_ONLY_TOOLS = git_tools.read_only_tools


def _scan_skills(root: Path) -> tuple[Path, ...]:
    """扫描指定根目录下所有含 ``SKILL.md`` 的子目录。

    @param root: 要扫描的根目录
    @return: 排序后的技能目录元组
    """
    if not root.is_dir():
        return ()

    return tuple(
        skill_dir
        for skill_dir in sorted(root.iterdir())
        if skill_dir.is_dir() and (skill_dir / "SKILL.md").is_file()
    )


def discover_project_skills() -> tuple[Path, ...]:
    """返回项目内及全局用户技能目录下所有含 ``SKILL.md`` 的技能目录。

    扫描路径（按优先级排序，同名目录去重保留先出现的）：
      1. ``.agents/skills``（项目内技能）
      2. ``C:\\Users\\tyx19\\.cursor\\skills``（全局用户技能）

    若某路径不存在则静默跳过。
    """
    seen: dict[Path, Path] = {}
    for root in (Path(".agents/skills"), Path(r"C:\Users\tyx19\.cursor\skills")):
        for skill_dir in _scan_skills(root):
            seen.setdefault(skill_dir.resolve(), skill_dir)

    return tuple(seen.values())


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
