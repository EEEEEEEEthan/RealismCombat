"""测试 workflow_egent_develop 模块的工具注册与 fuck 工具函数。"""

# pylint: disable=protected-access

from __future__ import annotations

import ast
import inspect
import re
from pathlib import Path

import pytest

import _common
import workflow_egent_develop


def _get_coding_source() -> ast.AsyncFunctionDef:
    """解析 workflow_egent_develop.coding 函数的 AST 节点。"""
    source = inspect.getsource(workflow_egent_develop.coding)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "coding":
            return node
    raise AssertionError("coding 函数未找到")


def _get_review_source() -> ast.AsyncFunctionDef:
    """解析 workflow_egent_develop.review 函数的 AST 节点。"""
    source = inspect.getsource(workflow_egent_develop.review)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "review":
            return node
    raise AssertionError("review 函数未找到")


def _find_local_function_def(
    function_name: str,
    parent_node: ast.AST,
) -> ast.FunctionDef | ast.AsyncFunctionDef | None:
    """在指定 AST 节点内部查找指定名称的内部函数/协程定义。"""
    for node in ast.walk(parent_node):
        if isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)) and node.name == function_name:
            return node
    return None


def _find_local_assign(
    target_name: str,
    parent_node: ast.AST,
) -> ast.Assign | None:
    """在指定 AST 节点内部查找对指定变量名的赋值。"""
    for node in ast.walk(parent_node):
        if isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name) and target.id == target_name:
                    return node
    return None


def _collect_tools_assignments(
    parent_node: ast.AST,
) -> list[ast.Assign]:
    """收集父节点中所有对 `.tools` 属性的赋值语句（按行号排序）。"""
    assignments = []
    for node in ast.walk(parent_node):
        if isinstance(node, ast.Assign) and any(
            isinstance(target, ast.Attribute) and target.attr == "tools"
            for target in node.targets
        ):
            assignments.append(node)
    assignments.sort(key=lambda n: n.lineno)
    return assignments


def _get_names_from_list(list_node: ast.List) -> set[str]:
    """从列表字面量中提取所有变量名（含 Starred 展开）。"""
    names: set[str] = set()
    for elem in list_node.elts:
        if isinstance(elem, ast.Name):
            names.add(elem.id)
        elif isinstance(elem, ast.Starred):
            node = elem.value
            parts = []
            while isinstance(node, ast.Attribute):
                parts.append(node.attr)
                node = node.value
            if isinstance(node, ast.Name):
                parts.append(node.id)
            names.add(".".join(reversed(parts)))
    return names


# ── coding() 内部 _run_pytest ───────────────────────────────────────────────


def test_coding_run_pytest_is_internal() -> None:
    """_run_pytest 应在 coding() 内部定义为嵌套函数。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("_run_pytest", coding_node)
    assert func_node is not None, "coding() 内部未找到 _run_pytest 函数定义"


# ── coding() 内部 fuck 测试 ──────────────────────────────────────────────────


def test_coding_fuck_from_make_fuck() -> None:
    """fuck 应通过 _common.make_fuck 赋值，而非定义为内部函数。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is None, "fuck 不应是 coding() 的内部函数定义，应来自 make_fuck"

    assign_node = _find_local_assign("fuck", coding_node)
    assert assign_node is not None, "coding() 内部未找到 fuck 赋值"
    assert isinstance(assign_node.value, ast.Call), "fuck 应为函数调用结果"
    call = assign_node.value
    assert isinstance(call.func, ast.Attribute) and call.func.attr == "make_fuck", (
        "fuck 应来自 _common.make_fuck"
    )


def test_coding_fuck_writes_with_timestamp_prefix() -> None:
    """coding 的 fuck 函数应写入带 [egent开发 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        fuck_fn = _common.make_fuck("[egent开发")
        result = fuck_fn("测试消息")
        assert result == "吐槽已记录。感谢反馈！"
        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert re.search(
            r"\[egent开发 \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] 测试消息\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        if fuck_path.exists():
            fuck_path.unlink()


def test_coding_fuck_in_both_tools_assignments() -> None:
    """coding 函数的两处非空 tools 赋值中均应包含 fuck。"""
    coding_node = _get_coding_source()
    tools_assignments = _collect_tools_assignments(coding_node)

    assert len(tools_assignments) >= 2, (
        f"coding 函数中至少应有 2 处 tools 赋值，实际找到 {len(tools_assignments)}"
    )

    non_empty = [
        a for a in tools_assignments
        if isinstance(a.value, ast.List) and a.value.elts
    ]
    assert len(non_empty) >= 2, (
        f"coding 函数中至少应有 2 处非空 tools 赋值，实际找到 {len(non_empty)}"
    )

    first_assignment = None
    second_assignment = None
    for assign in non_empty:
        names = _get_names_from_list(assign.value)
        if "run_pytest_test" in names:
            first_assignment = assign
        elif "fuck" in names and "run_pytest_test" not in names:
            second_assignment = assign

    assert first_assignment is not None, "未找到包含 run_pytest_test 的 tools 赋值"
    first_names = _get_names_from_list(first_assignment.value)
    assert "fuck" in first_names, (
        f"第一处 tools 赋值中应包含 fuck，实际有 {first_names}"
    )

    assert second_assignment is not None, "未找到第二处（仅含 fuck 的）tools 赋值"
    second_names = _get_names_from_list(second_assignment.value)
    assert "fuck" in second_names, (
        f"第二处 tools 赋值中应包含 fuck，实际有 {second_names}"
    )


def test_coding_run_pytest_test_defined() -> None:
    """coding 函数内部应定义 run_pytest_test 工具函数。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("run_pytest_test", coding_node)
    assert func_node is not None, (
        "coding() 内部未找到 run_pytest_test 函数定义"
    )


# ── review() 内部 fuck 测试 ──────────────────────────────────────────────────


def test_review_fuck_from_make_fuck() -> None:
    """review 的 fuck 应通过 _common.make_fuck 赋值。"""
    review_node = _get_review_source()
    func_node = _find_local_function_def("fuck", review_node)
    assert func_node is None, "fuck 不应是 review() 的内部函数定义"

    assign_node = _find_local_assign("fuck", review_node)
    assert assign_node is not None, "review() 内部未找到 fuck 赋值"
    assert isinstance(assign_node.value, ast.Call), "fuck 应为函数调用结果"
    call = assign_node.value
    assert isinstance(call.func, ast.Attribute) and call.func.attr == "make_fuck", (
        "fuck 应来自 _common.make_fuck"
    )


def test_review_fuck_writes_with_egent_review_prefix() -> None:
    """review 的 fuck 函数应写入带 [egent审查 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        fuck_fn = _common.make_fuck("[egent审查")
        result = fuck_fn("审查消息")
        assert result == "吐槽已记录。感谢反馈！"
        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert re.search(
            r"\[egent审查 \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] 审查消息\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        if fuck_path.exists():
            fuck_path.unlink()


def test_review_fuck_in_tools() -> None:
    """review 的 tools 赋值中应包含 fuck。"""
    review_node = _get_review_source()
    tools_assignments = _collect_tools_assignments(review_node)

    assert len(tools_assignments) >= 1, "review 函数中应有 tools 赋值"
    for assign in tools_assignments:
        if isinstance(assign.value, ast.List):
            names = _get_names_from_list(assign.value)
            if "fuck" in names:
                return

    pytest.fail("review 的 tools 赋值中未找到 fuck")


# ── _common.make_fuck 测试 ──────────────────────────────────────────────────


def test_make_fuck_signature() -> None:
    """make_fuck 应返回一个接受 str 参数并返回 str 的可调用对象。"""
    fuck_fn = _common.make_fuck("[test")
    assert callable(fuck_fn)

    # 检查返回函数签名
    sig = inspect.signature(fuck_fn)
    params = list(sig.parameters.values())
    assert len(params) == 1
    assert params[0].name == "msg"
    assert sig.return_annotation in (str, "str", inspect.Parameter.empty)


def test_make_fuck_writes_to_correct_path() -> None:
    """make_fuck 生成的函数应写入 .egent/.fuck.txt。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        fuck_fn = _common.make_fuck("[test")
        fuck_fn("路径测试")
        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
    finally:
        if fuck_path.exists():
            fuck_path.unlink()


def test_make_fuck_prefix_format() -> None:
    """make_fuck 生成的函数应写入带正确前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        fuck_fn = _common.make_fuck("[customTag")
        fuck_fn("hello")
        content = fuck_path.read_text(encoding="utf-8")
        # 格式: [customTag YYYY-MM-DD HH:MM:SS] hello\n
        assert re.search(
            r"\[customTag \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] hello\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        if fuck_path.exists():
            fuck_path.unlink()


# ── 模块级 ────────────────────────────────────────────────────────────────────


def test_coding_gave_up_replaced_by_runtime_error() -> None:
    """CodingGaveUp 异常类已移除，改用内置 RuntimeError。"""
    assert not hasattr(workflow_egent_develop, "CodingGaveUp"), (
        "CodingGaveUp 应已被移除"
    )


def test_pytest_timeout_constant_removed() -> None:
    """_PYTEST_TIMEOUT_SECONDS 常量应已移除（内联到使用处）。"""
    assert not hasattr(workflow_egent_develop, "_PYTEST_TIMEOUT_SECONDS"), (
        "_PYTEST_TIMEOUT_SECONDS 应已被移除"
    )


def test_path_permissions_functions_removed() -> None:
    """_egent_coder_path_permissions 和 _egent_reviewer_path_permissions 应已移除（内联）。"""
    assert not hasattr(workflow_egent_develop, "_egent_coder_path_permissions"), (
        "_egent_coder_path_permissions 应已被移除"
    )
    assert not hasattr(workflow_egent_develop, "_egent_reviewer_path_permissions"), (
        "_egent_reviewer_path_permissions 应已被移除"
    )


def test_run_pytest_removed_from_module_level() -> None:
    """_run_pytest 应已从模块级移除（成为 coding 内部方法）。"""
    assert not hasattr(workflow_egent_develop, "_run_pytest"), (
        "_run_pytest 应已从模块级移除"
    )
