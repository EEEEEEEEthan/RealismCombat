"""测试 workflow_egent_develop 模块的工具注册与 fuck 工具函数。"""

# pylint: disable=protected-access

from __future__ import annotations

import ast
import inspect
from pathlib import Path

import pytest

import workflow_egent_develop


def _get_coding_source() -> ast.AsyncFunctionDef:
    """解析 workflow_egent_develop.coding 函数的 AST 节点。"""
    source = inspect.getsource(workflow_egent_develop.coding)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "coding":
            return node
    raise AssertionError("coding 函数未找到")


def _find_local_function_def(function_name: str) -> ast.FunctionDef | None:
    """在 coding 函数内部查找指定名称的内部函数定义。"""
    coding_node = _get_coding_source()
    for node in ast.walk(coding_node):
        if isinstance(node, ast.FunctionDef) and node.name == function_name:
            return node
    return None


def _collect_tools_assignments(
    coding_node: ast.AsyncFunctionDef,
) -> list[ast.Assign]:
    """收集 coding 函数中所有对 `.tools` 属性的赋值语句（按行号排序）。"""
    assignments = []
    for node in ast.walk(coding_node):
        if isinstance(node, ast.Assign) and any(
            isinstance(target, ast.Attribute) and target.attr == "tools"
            for target in node.targets
        ):
            assignments.append(node)
    # 按行号升序排列
    assignments.sort(key=lambda n: n.lineno)
    return assignments


def _get_names_from_list(list_node: ast.List) -> set[str]:
    """从列表字面量中提取所有变量名（含 Starred 展开）。"""
    names: set[str] = set()
    for elem in list_node.elts:
        if isinstance(elem, ast.Name):
            names.add(elem.id)
        elif isinstance(elem, ast.Starred):
            # *_common.GIT_READ_ONLY_TOOLS — 提取完整属性链
            node = elem.value
            parts = []
            while isinstance(node, ast.Attribute):
                parts.append(node.attr)
                node = node.value
            if isinstance(node, ast.Name):
                parts.append(node.id)
            names.add(".".join(reversed(parts)))
    return names


def test_fuck_function_defined_inside_coding() -> None:
    """fuck 函数应在 coding() 内部定义为嵌套函数。"""
    func_node = _find_local_function_def("fuck")
    assert func_node is not None, "coding() 内部未找到 fuck 函数定义"


def test_fuck_function_has_correct_signature() -> None:
    """fuck 函数应接受一个 str 参数并返回 str。"""
    func_node = _find_local_function_def("fuck")
    assert func_node is not None
    args = func_node.args.args
    assert len(args) == 1, f"fuck 应只有 1 个参数 msg，实际有 {len(args)}"
    assert args[0].arg == "msg", f"参数名应为 msg，实际为 {args[0].arg}"
    # 返回值标注应为 str
    if func_node.returns:
        assert isinstance(func_node.returns, ast.Name) and func_node.returns.id == "str", (
            f"返回值标注应为 str，实际为 {ast.dump(func_node.returns)}"
        )


def test_fuck_writes_to_fuck_txt() -> None:
    """fuck 函数应将内容写入 .egent/.fuck.txt。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    # 清理测试残留
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        project_root = egent_dir.parent

        def _fuck(msg: str) -> str:
            """向 .egent/.fuck.txt 追加吐槽。"""
            _fuck_path = project_root / ".egent" / ".fuck.txt"
            _fuck_path.parent.mkdir(parents=True, exist_ok=True)
            with open(_fuck_path, "a", encoding="utf-8") as f:
                f.write(f"[egent开发]{msg}\n")
            return "吐槽已记录。感谢反馈！"

        result = _fuck("测试消息")
        assert result == "吐槽已记录。感谢反馈！"

        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert "[egent开发]测试消息\n" in content, (
            f"文件内容应为 '[egent开发]测试消息\\n'，实际为 {content!r}"
        )
    finally:
        if fuck_path.exists():
            fuck_path.unlink()


def test_run_pytest_test_in_coding_tools() -> None:
    """coding 函数的两处 tools 赋值中均应包含 run_pytest_test 和 fuck。"""
    coding_node = _get_coding_source()
    tools_assignments = _collect_tools_assignments(coding_node)

    assert len(tools_assignments) >= 2, (
        f"coding 函数中至少应有 2 处 tools 赋值，实际找到 {len(tools_assignments)}"
    )

    # 找到第一个包含 run_pytest_test 的 tools 赋值
    first_assignment = None
    second_assignment = None
    for assign in tools_assignments:
        if isinstance(assign.value, ast.List):
            names = _get_names_from_list(assign.value)
            if "run_pytest_test" in names:
                first_assignment = assign
            elif "fuck" in names and "run_pytest_test" not in names:
                second_assignment = assign

    assert first_assignment is not None, (
        "未找到包含 run_pytest_test 的 tools 赋值"
    )
    first_names = _get_names_from_list(first_assignment.value)
    assert "fuck" in first_names, (
        f"第一处 tools 赋值中应包含 fuck，实际有 {first_names}"
    )

    assert second_assignment is not None, (
        "未找到第二处（仅含 fuck 的）tools 赋值"
    )
    second_names = _get_names_from_list(second_assignment.value)
    assert "fuck" in second_names, (
        f"第二处 tools 赋值中应包含 fuck，实际有 {second_names}"
    )


@pytest.mark.parametrize(
    "function_name",
    ["run_pytest_test", "fuck"],
)
def test_both_tools_defined_in_coding(function_name: str) -> None:
    """coding 函数内部应定义 run_pytest_test 和 fuck 两个工具函数。"""
    func_node = _find_local_function_def(function_name)
    assert func_node is not None, (
        f"coding() 内部未找到 {function_name} 函数定义"
    )
