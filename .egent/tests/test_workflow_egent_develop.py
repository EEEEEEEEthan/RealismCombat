"""测试 workflow_egent_develop 模块的工具注册与 fuck 工具函数。"""

# pylint: disable=protected-access

from __future__ import annotations

import ast
import inspect
import re
from datetime import datetime
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
) -> ast.FunctionDef | None:
    """在指定 AST 节点内部查找指定名称的内部函数定义。"""
    for node in ast.walk(parent_node):
        if isinstance(node, ast.FunctionDef) and node.name == function_name:
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


# ── coding() 内部 fuck 测试 ──────────────────────────────────────────────────


def test_coding_fuck_function_defined() -> None:
    """fuck 函数应在 coding() 内部定义为嵌套函数。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is not None, "coding() 内部未找到 fuck 函数定义"


def test_coding_fuck_function_has_correct_signature() -> None:
    """fuck 函数应接受一个 str 参数并返回 str。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is not None
    args = func_node.args.args
    assert len(args) == 1, f"fuck 应只有 1 个参数 msg，实际有 {len(args)}"
    assert args[0].arg == "msg", f"参数名应为 msg，实际为 {args[0].arg}"
    if func_node.returns:
        assert isinstance(func_node.returns, ast.Name) and func_node.returns.id == "str", (
            f"返回值标注应为 str，实际为 {ast.dump(func_node.returns)}"
        )


def test_coding_fuck_writes_with_timestamp_prefix() -> None:
    """coding 的 fuck 函数应写入带 [egent开发 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        project_root = egent_dir.parent

        def _fuck(msg: str) -> str:
            """模拟 coding 中的 fuck。"""
            _fuck_path = project_root / ".egent" / ".fuck.txt"
            _fuck_path.parent.mkdir(parents=True, exist_ok=True)
            timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            with open(_fuck_path, "a", encoding="utf-8") as f:
                f.write(f"[egent开发 {timestamp}] {msg}\n")
            return "吐槽已记录。感谢反馈！"

        result = _fuck("测试消息")
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


def test_coding_fuck_uses_datetime_now() -> None:
    """coding 中 fuck 函数体内应包含 datetime.now() 调用。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is not None

    # 检查函数体内是否有 datetime.now 调用
    has_datetime_now = any(
        isinstance(node, ast.Call)
        and isinstance(node.func, ast.Attribute)
        and node.func.attr == "now"
        and isinstance(node.func.value, ast.Name)
        and node.func.value.id == "datetime"
        for node in ast.walk(func_node)
    )
    assert has_datetime_now, "fuck 函数体内应调用 datetime.now()"


def test_coding_fuck_uses_strftime() -> None:
    """coding 中 fuck 函数体内应包含 strftime('%Y-%m-%d %H:%M:%S') 调用。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is not None

    has_strftime = any(
        isinstance(node, ast.Call)
        and isinstance(node.func, ast.Attribute)
        and node.func.attr == "strftime"
        for node in ast.walk(func_node)
    )
    assert has_strftime, "fuck 函数体内应调用 strftime"


def test_coding_fuck_in_both_tools_assignments() -> None:
    """coding 函数的两处非空 tools 赋值中均应包含 fuck。"""
    coding_node = _get_coding_source()
    tools_assignments = _collect_tools_assignments(coding_node)

    assert len(tools_assignments) >= 2, (
        f"coding 函数中至少应有 2 处 tools 赋值，实际找到 {len(tools_assignments)}"
    )

    # 只检查有内容的列表赋值
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


@pytest.mark.parametrize("function_name", ["run_pytest_test", "fuck"])
def test_coding_both_tools_defined(function_name: str) -> None:
    """coding 函数内部应定义 run_pytest_test 和 fuck 两个工具函数。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def(function_name, coding_node)
    assert func_node is not None, (
        f"coding() 内部未找到 {function_name} 函数定义"
    )


# ── review() 内部 fuck 测试 ──────────────────────────────────────────────────


def test_review_fuck_function_defined() -> None:
    """fuck 函数应在 review() 内部定义为嵌套函数。"""
    review_node = _get_review_source()
    func_node = _find_local_function_def("fuck", review_node)
    assert func_node is not None, "review() 内部未找到 fuck 函数定义"


def test_review_fuck_has_correct_signature() -> None:
    """review 中的 fuck 函数应接受一个 str 参数并返回 str。"""
    review_node = _get_review_source()
    func_node = _find_local_function_def("fuck", review_node)
    assert func_node is not None
    args = func_node.args.args
    assert len(args) == 1, f"fuck 应只有 1 个参数 msg，实际有 {len(args)}"
    assert args[0].arg == "msg", f"参数名应为 msg，实际为 {args[0].arg}"
    if func_node.returns:
        assert isinstance(func_node.returns, ast.Name) and func_node.returns.id == "str", (
            f"返回值标注应为 str，实际为 {ast.dump(func_node.returns)}"
        )


def test_review_fuck_uses_egent_review_prefix() -> None:
    """review 的 fuck 应使用 [egent审查 前缀。"""
    review_node = _get_review_source()
    func_node = _find_local_function_def("fuck", review_node)
    assert func_node is not None

    # 检查字符串中是否包含 [egent审查
    has_prefix = any(
        isinstance(node, ast.JoinedStr)
        and any(
            isinstance(v, ast.Constant) and "[egent审查" in str(v.value)
            for v in node.values
        )
        for node in ast.walk(func_node)
    )
    assert has_prefix, "fuck 函数应写入 [egent审查 ...] 前缀"


def test_review_fuck_uses_datetime_now() -> None:
    """review 中 fuck 函数体内应包含 datetime.now() 调用。"""
    review_node = _get_review_source()
    func_node = _find_local_function_def("fuck", review_node)
    assert func_node is not None

    has_datetime_now = any(
        isinstance(node, ast.Call)
        and isinstance(node.func, ast.Attribute)
        and node.func.attr == "now"
        and isinstance(node.func.value, ast.Name)
        and node.func.value.id == "datetime"
        for node in ast.walk(func_node)
    )
    assert has_datetime_now, "review fuck 函数体内应调用 datetime.now()"


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


def test_review_fuck_writes_with_egent_review_prefix() -> None:
    """review 的 fuck 函数应写入带 [egent审查 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    if fuck_path.exists():
        fuck_path.unlink()

    try:
        def _fuck(msg: str) -> str:
            """模拟 review 中的 fuck。"""
            _fuck_path = egent_dir / ".fuck.txt"
            _fuck_path.parent.mkdir(parents=True, exist_ok=True)
            timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
            with open(_fuck_path, "a", encoding="utf-8") as f:
                f.write(f"[egent审查 {timestamp}] {msg}\n")
            return "吐槽已记录。感谢反馈！"

        result = _fuck("审查吐槽")
        assert result == "吐槽已记录。感谢反馈！"

        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert re.search(
            r"\[egent审查 \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] 审查吐槽\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        if fuck_path.exists():
            fuck_path.unlink()
