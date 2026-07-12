"""测试 workflow_gameplay_develop 模块的 fuck 工具函数注册与 system prompt 内容。"""

# pylint: disable=protected-access

from __future__ import annotations

import ast
import inspect
import re
from pathlib import Path

import pytest

import _common
import workflow_gameplay_develop


def _get_coding_source() -> ast.AsyncFunctionDef:
    """解析 workflow_gameplay_develop.coding 函数的 AST 节点。"""
    source = inspect.getsource(workflow_gameplay_develop.coding)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "coding":
            return node
    raise AssertionError("coding 函数未找到")


def _get_review_source() -> ast.AsyncFunctionDef:
    """解析 workflow_gameplay_develop.review 函数的 AST 节点。"""
    source = inspect.getsource(workflow_gameplay_develop.review)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "review":
            return node
    raise AssertionError("review 函数未找到")


def _get_test_source() -> ast.AsyncFunctionDef:
    """解析 workflow_gameplay_develop.test 函数的 AST 节点。"""
    source = inspect.getsource(workflow_gameplay_develop.test)
    tree = ast.parse(source)
    for node in ast.walk(tree):
        if isinstance(node, ast.AsyncFunctionDef) and node.name == "test":
            return node
    raise AssertionError("test 函数未找到")


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


def _collect_tools_assignments(parent_node: ast.AST) -> list[ast.Assign]:
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


def _restore_fuck_file(
    fuck_path: Path,
    original: str,
    msg_identifier: str,
) -> None:
    """移除 .fuck.txt 中包含 msg_identifier 的行，恢复文件到测试前状态。"""
    if not fuck_path.exists():
        return
    remaining = [
        l for l in fuck_path.read_text(encoding="utf-8").splitlines(keepends=True)
        if msg_identifier not in l
    ]
    if not original and not remaining:
        fuck_path.unlink()
    else:
        fuck_path.write_text("".join(remaining), encoding="utf-8")


# ── coding() 内部 fuck 测试 ──────────────────────────────────────────────────


def test_coding_fuck_from_make_fuck() -> None:
    """coding 的 fuck 应通过 _common.make_fuck 赋值。"""
    coding_node = _get_coding_source()
    func_node = _find_local_function_def("fuck", coding_node)
    assert func_node is None, "fuck 不应是 coding() 的内部函数定义"

    assign_node = _find_local_assign("fuck", coding_node)
    assert assign_node is not None, "coding() 内部未找到 fuck 赋值"
    assert isinstance(assign_node.value, ast.Call), "fuck 应为函数调用结果"
    call = assign_node.value
    assert isinstance(call.func, ast.Attribute) and call.func.attr == "make_fuck", (
        "fuck 应来自 _common.make_fuck"
    )


def test_coding_fuck_uses_gameplay_develop_prefix() -> None:
    """coding 的 fuck 应使用 [gameplay开发 前缀。"""
    coding_node = _get_coding_source()
    assign_node = _find_local_assign("fuck", coding_node)
    assert assign_node is not None

    # make_fuck 的参数应包含 [gameplay开发
    call = assign_node.value
    assert isinstance(call, ast.Call)
    if call.args:
        first_arg = call.args[0]
        if isinstance(first_arg, ast.Constant) and isinstance(first_arg.value, str):
            assert "[gameplay开发" in first_arg.value, (
                f"make_fuck 参数应包含 [gameplay开发，实际为 {first_arg.value!r}"
            )


def test_coding_fuck_writes_with_gameplay_develop_prefix() -> None:
    """coding 的 fuck 函数应写入带 [gameplay开发 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    original = fuck_path.read_text(encoding="utf-8") if fuck_path.exists() else ""

    try:
        fuck_fn = _common.make_fuck("[gameplay开发")
        result = fuck_fn("测试消息")
        assert result == "反馈已记录。感谢贡献！"

        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert re.search(
            r"\[gameplay开发 \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] 测试消息\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        _restore_fuck_file(fuck_path, original, "测试消息")


def test_coding_fuck_in_both_tools_assignments() -> None:
    """coding 函数的两处主要 tools 赋值中均应包含 fuck。"""
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
    for assign in non_empty:
        names = _get_names_from_list(assign.value)
        assert "fuck" in names, (
            f"每个非空 tools 赋值中均应包含 fuck，实际有 {names}"
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


def test_review_fuck_uses_gameplay_review_prefix() -> None:
    """review 的 fuck 应使用 [gameplay审查 前缀。"""
    review_node = _get_review_source()
    assign_node = _find_local_assign("fuck", review_node)
    assert assign_node is not None

    call = assign_node.value
    assert isinstance(call, ast.Call)
    if call.args:
        first_arg = call.args[0]
        if isinstance(first_arg, ast.Constant) and isinstance(first_arg.value, str):
            assert "[gameplay审查" in first_arg.value, (
                f"make_fuck 参数应包含 [gameplay审查，实际为 {first_arg.value!r}"
            )


def test_review_fuck_writes_with_gameplay_review_prefix() -> None:
    """review 的 fuck 函数应写入带 [gameplay审查 YYYY-MM-DD HH:MM:SS] 前缀的内容。"""
    egent_dir = Path(__file__).resolve().parent.parent
    fuck_path = egent_dir / ".fuck.txt"
    original = fuck_path.read_text(encoding="utf-8") if fuck_path.exists() else ""

    try:
        fuck_fn = _common.make_fuck("[gameplay审查")
        result = fuck_fn("消息")
        assert result == "反馈已记录。感谢贡献！"
        assert fuck_path.exists(), ".fuck.txt 文件应被创建"
        content = fuck_path.read_text(encoding="utf-8")
        assert re.search(
            r"\[gameplay审查 \d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\] 消息\n",
            content,
        ), f"内容格式错误: {content!r}"
    finally:
        _restore_fuck_file(fuck_path, original, "消息")


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


# ── test() 内部 ──────────────────────────────────────────────────────────────


def test_test_launch_game_session_in_closure() -> None:
    """test 函数的 launch_game 闭包内应调用 godot_game_tools.launch_game_session。"""
    test_node = _get_test_source()

    launch_game_func = _find_local_function_def("launch_game", test_node)
    assert launch_game_func is not None, "test 函数体内应定义 launch_game 闭包"

    has_session_call = any(
        isinstance(node, ast.Call)
        and isinstance(node.func, ast.Attribute)
        and node.func.attr == "launch_game_session"
        for node in ast.walk(launch_game_func)
    )
    assert has_session_call, "launch_game 闭包内应调用 godot_game_tools.launch_game_session"


def test_test_launch_game_used_in_tools() -> None:
    """test 函数的 tools 列表应使用本地 launch_game 闭包而非 godot_game_tools.launch_game。"""
    test_node = _get_test_source()

    tools_assignments = _collect_tools_assignments(test_node)
    assert len(tools_assignments) >= 1, "test 函数中应有 tools 赋值"

    for assign in tools_assignments:
        if isinstance(assign.value, ast.List):
            names = _get_names_from_list(assign.value)
            assert "launch_game" in names, "tools 列表中应包含 launch_game 闭包"
            assert "godot_game_tools.launch_game" not in names, (
                "tools 列表中不应包含 godot_game_tools.launch_game，应使用本地闭包"
            )


def test_test_has_processes_list() -> None:
    """test 函数体内应定义 _processes 列表变量。"""
    test_node = _get_test_source()

    has_var = any(
        (
            isinstance(node, ast.Assign)
            and any(
                isinstance(target, ast.Name) and target.id == "_processes"
                for target in node.targets
            )
        )
        or (
            isinstance(node, ast.AnnAssign)
            and isinstance(node.target, ast.Name)
            and node.target.id == "_processes"
        )
        for node in ast.walk(test_node)
    )
    assert has_var, "test 函数体内应定义 _processes 列表"


def test_test_finally_kills_processes() -> None:
    """test 函数的 finally 块应遍历 _processes 逐个 kill。"""
    test_node = _get_test_source()
    # 找到 test 函数中 finally 语句
    for node in ast.walk(test_node):
        if isinstance(node, ast.Try):
            for handler in node.handlers:
                if handler.type is None:  # bare except
                    break
            else:
                # 检查 finally 块中是否有遍历 _processes
                if node.finalbody:
                    source = ast.unparse(node.finalbody)  # type: ignore[arg-type]
                    assert "_processes" in source, (
                        "finally 块中应引用 _processes"
                    )


# ── test() system prompt 内容 ────────────────────────────────────────────────


def _get_test_system_prompt() -> str:
    """提取 test 函数中 tester.add_message('system', ...) 的完整字符串。"""
    test_node = _get_test_source()
    source = ast.unparse(test_node)  # type: ignore[arg-type]

    # 在源码字符串中定位 system prompt 段落
    # 我们提取从 "你是这个项目的白盒测试员" 到最后一个 "fuck 工具吐槽反馈。\n" 之间的内容
    lines = source.splitlines()
    prompt_lines: list[str] = []
    in_prompt = False
    for line in lines:
        # 跳过引号包裹的边界
        stripped = line.strip()
        if '你是这个项目的白盒测试员' in stripped:
            in_prompt = True
        if in_prompt:
            prompt_lines.append(stripped)
        if in_prompt and 'fuck 工具吐槽反馈。' in stripped:
            break

    return "\n".join(prompt_lines)


def test_test_execute_convention_uses_void_return() -> None:
    """execute 脚本约定中 run 应声明 -> void。"""
    prompt = _get_test_system_prompt()
    assert "static func run(scene_tree: SceneTree) -> void" in prompt, (
        "execute 脚本约定应使用 -> void 而非 -> Variant"
    )


def test_test_execute_convention_uses_print() -> None:
    """execute 脚本约定应描述用 print() 输出关键值。"""
    prompt = _get_test_system_prompt()
    assert "用 print() 输出关键值" in prompt, (
        "应描述用 print() 输出关键值而非 return"
    )
    assert "不要用 return 返回数据" in prompt, (
        "应明确说明不要用 return 返回数据"
    )


def test_test_example1_uses_void_return() -> None:
    """示例 1 的 run 方法应使用 -> void。"""
    prompt = _get_test_system_prompt()
    assert "static func run(scene_tree: SceneTree) -> void:" in prompt


def test_test_example1_uses_print_instead_of_return() -> None:
    """示例 1 应使用简洁的 print 输出结果，不含 _Common 等多余内容。"""
    prompt = _get_test_system_prompt()
    example1 = prompt.split("### 示例 2", maxsplit=1)[0]

    assert 'print("paused: ", scene_tree.paused)' in example1, (
        "示例 1 应 print paused 状态"
    )
    assert "_Common" not in example1, (
        "示例 1 不应包含 _Common"
    )
    assert '"FAIL"' not in example1, (
        "示例 1 不应包含 FAIL 错误处理"
    )
    assert 'return {"ok"' not in example1, (
        "示例 1 不应包含 return 字典"
    )


def test_test_example2_uses_void_return() -> None:
    """示例 2 的 run 方法应使用 -> void。"""
    prompt = _get_test_system_prompt()
    assert "static func run(scene_tree: SceneTree) -> void:" in prompt

    examples = prompt.split("### 示例 2", maxsplit=1)
    assert len(examples) >= 2
    example2 = examples[1]
    assert "static func run(scene_tree: SceneTree) -> void:" in example2


def test_test_example2_uses_print_instead_of_return() -> None:
    """示例 2 应使用 create_timer + await 输出结果，不含手动帧累加。"""
    prompt = _get_test_system_prompt()
    examples = prompt.split("### 示例 2", maxsplit=1)
    assert len(examples) >= 2
    example2 = examples[1]

    assert "create_timer(1.0, false, false, true)" in example2, (
        "示例 2 应使用 create_timer(1.0, false, false, true)"
    )
    assert "await timer.timeout" in example2, (
        "示例 2 应 await timer.timeout"
    )
    assert 'print("1 second passed")' in example2, (
        "示例 2 应 print 1 second passed"
    )
    assert "_Common" not in example2, (
        "示例 2 不应包含 _Common"
    )
    assert "elapsed_sec" not in example2, (
        "示例 2 不应包含 elapsed_sec"
    )
    assert 'return {"ok"' not in example2, (
        "示例 2 不应包含 return 字典"
    )


# ── 模块级 ────────────────────────────────────────────────────────────────────


def test_coding_gave_up_replaced_by_runtime_error() -> None:
    """CodingGaveUp 异常类已移除，改用内置 RuntimeError。"""
    assert not hasattr(workflow_gameplay_develop, "CodingGaveUp"), (
        "CodingGaveUp 应已被移除"
    )
