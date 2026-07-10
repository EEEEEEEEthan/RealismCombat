"""``_common`` 模块单元测试。"""

from __future__ import annotations

from pathlib import Path

import pytest

import _common


class TestMakeFuck:
    """``make_fuck`` 闭包创建与行为测试。"""

    def test_returns_callable(self) -> None:
        """make_fuck 应返回一个可调用对象。"""
        fuck_fn = _common.make_fuck("[test]")
        assert callable(fuck_fn)

    def test_fuck_docstring_contains_optimization_signal_phrasing(self) -> None:
        """fuck 闭包的文档字符串应包含优化信号相关的措辞。"""
        fuck_fn = _common.make_fuck("[test]")
        doc = fuck_fn.__doc__
        assert doc is not None
        assert "提交工具/API/架构的改进反馈" in doc
        assert "优化信号" in doc
        assert "影响范围" in doc
        assert "期望的改进方向" in doc
        assert "这不是抱怨" in doc

    def test_fuck_writes_to_fuck_txt(self, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
        """调用 fuck 闭包应将反馈写入 .fuck.txt。"""
        fake_egent_dir = tmp_path / "egent"
        fake_egent_dir.mkdir()
        fake_file = fake_egent_dir / "_common.py"
        monkeypatch.setattr(_common, "__file__", str(fake_file))

        fuck_fn = _common.make_fuck("[test]")
        result = fuck_fn("单元测试反馈消息")
        assert "反馈已记录" in result

        fuck_path = fake_egent_dir / ".fuck.txt"
        assert fuck_path.exists()
        content = fuck_path.read_text(encoding="utf-8")
        assert "[test]" in content
        assert "单元测试反馈消息" in content

    def test_fuck_docstring_parameter_format(self) -> None:
        """fuck 闭包的 @param msg 应包含具体问题描述/影响/期望方向。"""
        fuck_fn = _common.make_fuck("[test]")
        doc = fuck_fn.__doc__
        assert doc is not None
        assert "@param msg:" in doc
        assert "具体问题描述" in doc


class TestScanSkills:
    """``_scan_skills`` 扫描逻辑测试。"""

    def test_empty_when_root_not_exists(self, tmp_path: Path) -> None:
        """根目录不存在时应返回空元组。"""
        non_existent = tmp_path / "no_such_dir"
        assert not non_existent.exists()
        result = _common._scan_skills(non_existent)  # pylint: disable=protected-access
        assert not result

    def test_empty_when_no_skill_dirs(self, tmp_path: Path) -> None:
        """根目录存在但无含 SKILL.md 的子目录时应返回空元组。"""
        root = tmp_path / "skills"
        root.mkdir()
        (root / "some_dir").mkdir()
        result = _common._scan_skills(root)  # pylint: disable=protected-access
        assert not result

    def test_returns_dirs_with_skill_md(self, tmp_path: Path) -> None:
        """应返回所有含 SKILL.md 的子目录，按名称排序。"""
        root = tmp_path / "skills"
        root.mkdir()

        (root / "skill_b").mkdir()
        (root / "skill_b" / "SKILL.md").write_text("")
        (root / "skill_a").mkdir()
        (root / "skill_a" / "SKILL.md").write_text("")
        (root / "no_skill").mkdir()

        result = _common._scan_skills(root)  # pylint: disable=protected-access
        assert len(result) == 2
        assert result[0].name == "skill_a"
        assert result[1].name == "skill_b"

    def test_ignores_files_not_dirs(self, tmp_path: Path) -> None:
        """根目录下的普通文件应被忽略。"""
        root = tmp_path / "skills"
        root.mkdir()
        (root / "some_file.py").write_text("# not a dir")
        (root / "skill_z").mkdir()
        (root / "skill_z" / "SKILL.md").write_text("")
        result = _common._scan_skills(root)  # pylint: disable=protected-access
        assert len(result) == 1
        assert result[0].name == "skill_z"


class TestDiscoverProjectSkills:
    """``discover_project_skills`` 集成逻辑测试。

    通过 mock ``_scan_skills`` 来模拟不同来源的技能目录，
    避免依赖真实文件系统路径。
    """

    @staticmethod
    def _make_skill_dir(tmp_path: Path, rel: str) -> Path:
        """在 tmp_path 下创建含 SKILL.md 的技能目录并返回其 Path。"""
        d = tmp_path / rel
        d.mkdir(parents=True)
        (d / "SKILL.md").write_text("")
        return d

    @staticmethod
    def _is_project_skills_root(root: Path) -> bool:
        """判断 root 是否为项目内 ``.agents/skills`` 路径。"""
        return ".agents" in root.parts and "skills" in root.parts

    @staticmethod
    def _is_global_skills_root(root: Path) -> bool:
        """判断 root 是否为全局用户 ``skills`` 路径。"""
        return ".cursor" in root.parts and "skills" in root.parts

    def test_only_project_skills(self, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
        """仅项目内 skills 存在时，应正确返回。"""
        proj_skill = self._make_skill_dir(tmp_path, "agents/skills/my_skill")

        def mock_scan_skills(root: Path) -> tuple[Path, ...]:
            if self._is_project_skills_root(root):
                return (proj_skill,)
            return ()

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        assert result == (proj_skill,)

    def test_global_skills_merged(self, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
        """全局 skills 与项目 skills 合并返回。"""
        proj_skill = self._make_skill_dir(tmp_path, "agents/skills/proj_skill")
        global_skill = self._make_skill_dir(tmp_path, "global/global_skill")

        calls: list[Path] = []

        def mock_scan_skills(root: Path) -> tuple[Path, ...]:
            calls.append(root)
            if self._is_project_skills_root(root):
                return (proj_skill,)
            if self._is_global_skills_root(root):
                return (global_skill,)
            return ()

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        assert set(result) == {proj_skill, global_skill}
        assert len(calls) == 2

    def test_dedup_by_resolved_path(self, tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
        """同名技能目录通过 resolved 路径去重。

        当项目内和全局指向**同一个物理目录**时，应只返回一次。
        """
        same_dir = self._make_skill_dir(tmp_path, "shared/common_skill")

        def mock_scan_skills(_root: Path) -> tuple[Path, ...]:
            return (same_dir,)  # 两个来源返回同一目录对象

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        assert len(result) == 1
        assert result[0] is same_dir

    def test_global_skills_not_exists(self, monkeypatch: pytest.MonkeyPatch) -> None:
        """全局 skills 目录不存在时静默跳过，仅返回项目内技能。"""
        proj_only = Path("/fake/proj_skill")

        def mock_scan_skills(root: Path) -> tuple[Path, ...]:
            if self._is_project_skills_root(root):
                return (proj_only,)
            return ()

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        assert len(result) == 1
        assert result[0] is proj_only

    def test_no_skills_at_all(self, monkeypatch: pytest.MonkeyPatch) -> None:
        """任何路径下都无技能时返回空元组。"""
        monkeypatch.setattr(_common, "_scan_skills", lambda _: ())
        result = _common.discover_project_skills()
        assert not result

    def test_project_agents_skills_not_exists(self, monkeypatch: pytest.MonkeyPatch) -> None:
        """项目 agents/skills 目录不存在时也应静默跳过，仅返回全局技能。"""
        global_only = Path("/fake/global_skill")

        def mock_scan_skills(root: Path) -> tuple[Path, ...]:
            if self._is_global_skills_root(root):
                return (global_only,)
            return ()

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        assert len(result) == 1
        assert result[0] is global_only

    def test_project_skills_take_priority(self, monkeypatch: pytest.MonkeyPatch) -> None:
        """当项目内和全局存在同名（resolve 后相同）技能时，项目内优先（先出现）。"""
        proj_skill = Path("/fake/proj/skill_a")
        global_skill = Path("/fake/global/skill_a")

        def mock_scan_skills(root: Path) -> tuple[Path, ...]:
            if self._is_project_skills_root(root):
                return (proj_skill,)
            if self._is_global_skills_root(root):
                return (global_skill,)
            return ()

        monkeypatch.setattr(_common, "_scan_skills", mock_scan_skills)
        result = _common.discover_project_skills()
        # 两个路径不同，应都返回
        assert len(result) == 2
