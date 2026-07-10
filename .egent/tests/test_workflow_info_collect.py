"""测试 workflow_info_collect 模块的可导入性与签名。"""

from __future__ import annotations

import inspect

import workflow_info_collect


def test_begin_info_collect_workflow_is_importable() -> None:
    """begin_info_collect_workflow 应从模块导入并可调用。"""
    assert hasattr(workflow_info_collect, "begin_info_collect_workflow"), (
        "模块应导出 begin_info_collect_workflow"
    )
    assert callable(workflow_info_collect.begin_info_collect_workflow), (
        "begin_info_collect_workflow 应为可调用对象"
    )


def test_begin_info_collect_workflow_is_async() -> None:
    """begin_info_collect_workflow 应为 async 函数。"""
    assert inspect.iscoroutinefunction(
        workflow_info_collect.begin_info_collect_workflow,
    ), "begin_info_collect_workflow 应为 async 函数"


def test_begin_info_collect_workflow_signature() -> None:
    """begin_info_collect_workflow 签名应为 (description: str) -> tuple[bool, str]。"""
    sig = inspect.signature(workflow_info_collect.begin_info_collect_workflow)
    params = list(sig.parameters.values())
    assert len(params) == 1, (
        f"begin_info_collect_workflow 应接受 1 个参数，实际有 {len(params)}"
    )
    param = params[0]
    assert param.name == "description", (
        f"参数名应为 description，实际为 {param.name}"
    )
    # 检查返回类型注解
    return_annotation = sig.return_annotation
    assert return_annotation is not inspect.Parameter.empty, "应有返回类型注解"
    # 使用 from __future__ import annotations 时注解为字符串
    expected = "tuple[bool, str]"
    assert str(return_annotation) == expected, (
        f"返回类型应为 {expected}，实际为 {return_annotation}"
    )
