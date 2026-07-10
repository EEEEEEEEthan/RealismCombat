"""
对 egent.builtin_tools.path_validator 中 fnmatch 匹配行为的测试。

背景：list_path_permissions 将可编辑白名单显示为 `C, :, /, P, r, ...`（单字符），
怀疑某处对 whitelist tuple 做了字符级迭代。

根因分析（path_validator.py 源码）：
──────────────────────────────────
1. _format_pattern_list(patterns: tuple[str, ...]) 使用 `", ".join(patterns)` 拼接。
   如果 patterns 实际是 str（而非 tuple），join 会逐字符迭代，导致输出 "C, :, /, ..."。

2. path_matches_patterns(path, patterns) 使用 `for pattern in patterns` 迭代。
   若 patterns 是 str，也会逐字符迭代，fnmatch 匹配会完全失效。

3. 触发条件：main.py 中构造 PathPermissionRule 时：
       whitelist=(f"{project_root}/.agents/*")   # ← 缺少尾部逗号！
   在 Python 中，(expr) 是分组括号，不会创建元组；必须加逗号才是元组字面量。
   因此 whitelist 被赋值为普通字符串，而非单元素元组。

4. 同理，blacklist=("*.pyc") 也会踩同样的坑。

修复方案：
- main.py 中已补上尾部逗号：whitelist=(f"{project_root}/.agents/*",)
- path_validator.py 建议在 PathPermissionRule.__post_init__ 中增加类型断言，
  避免类型静默漂移（但因在 pip 安装包中，无法直接编辑）。
  可参考以下防御代码插在 dataclass 中：

      def __post_init__(self) -> None:
          if isinstance(self.whitelist, str):
              msg = (
                  f"whitelist 应为 tuple[str, ...]，"
                  f"但收到 str: '{self.whitelist}'。"
                  f"可能是单元素元组遗漏了尾部逗号。"
              )
              raise TypeError(msg)
          if isinstance(self.blacklist, str):
              msg = (
                  f"blacklist 应为 tuple[str, ...]，"
                  f"但收到 str: '{self.blacklist}'。"
              )
              raise TypeError(msg)
"""

from __future__ import annotations

from pathlib import Path

import egent.builtin_tools.path_validator as pv

# 测试用的固定路径（不依赖运行时 project_root）
_PROJECT_ROOT = "C:/Projects/RealismCombat"
_AGENTS_GLOB = f"{_PROJECT_ROOT}/.agents/*"
_EGENT_FILE = f"{_PROJECT_ROOT}/.egent/.fuck.txt"
_AGENTS_FILE = f"{_PROJECT_ROOT}/.agents/foo.md"


def test_whitelist_proper_tuple_no_char_iteration():
    """验证 whitelist 为 (str,) 元组时不会被拆成字符。"""
    rule = pv.PathPermissionRule(
        whitelist=(_AGENTS_GLOB,),
        blacklist=(),
    )
    whitelist = rule.whitelist
    assert isinstance(whitelist, tuple), (
        f"whitelist 不是 tuple，而是 {type(whitelist).__name__}！"
    )
    assert len(whitelist) == 1, (
        f"whitelist 应有 1 个元素，实际有 {len(whitelist)} 个元素"
    )
    assert whitelist[0] == _AGENTS_GLOB, (
        f"whitelist[0] 应是 '{_AGENTS_GLOB}'，实际是 '{whitelist[0]}'"
    )


def test_agents_file_allowed():
    """测试白名单内的 .agents/foo.md 应被允许。"""
    rule = pv.PathPermissionRule(
        whitelist=(_AGENTS_GLOB,),
        blacklist=(),
    )
    path = Path(_AGENTS_FILE)
    assert rule.allows(path), (
        f"'{_AGENTS_FILE}' 应匹配 whitelist '{_AGENTS_GLOB}'"
    )


def test_egent_file_not_allowed():
    """测试白名单外的 .egent/.fuck.txt 应不被允许。"""
    rule = pv.PathPermissionRule(
        whitelist=(_AGENTS_GLOB,),
        blacklist=(),
    )
    path = Path(_EGENT_FILE)
    assert not rule.allows(path), (
        f"'{_EGENT_FILE}' 不应匹配 whitelist '{_AGENTS_GLOB}'"
    )


def test_whitelist_string_bug_reproduction():
    """
    复现原始 bug：当 whitelist 传字符串而非元组时的异常行为。

    这模拟了 main.py 中遗漏尾部逗号的情形：
        whitelist=(f"{project_root}/.agents/*")   # ← 不带逗号！
    (expr) 在 Python 中是分组括号，值仍是字符串。

    字符串被逐字符迭代的后果：
    1. whitelist 类型变为 str，长度是字符串的字符数（41）。
    2. path_matches_patterns 逐个字符调用 fnmatch，
       由于末尾字符 '*' 匹配一切，导致白名单实际上允许了所有路径！
    """
    # 故意传字符串（模拟 bug）
    bad_rule = pv.PathPermissionRule(
        whitelist=_AGENTS_GLOB,  # 直接传字符串，不是元组！
        blacklist=(),
    )
    whitelist = bad_rule.whitelist

    # bug①：类型是 str 而非 tuple
    assert isinstance(whitelist, str), (
        "模拟 bug 时 whitelist 应为 str，但实际不是"
    )
    # bug②：长度是字符串字符数（不是 1）
    assert len(whitelist) == len(_AGENTS_GLOB), (
        f"字符串长度应为 {len(_AGENTS_GLOB)}，实际为 {len(whitelist)}"
    )

    # bug③：字符串 whitelist 导致 fnmatch 逐字符迭代，
    # 末尾的 '*' 意外匹配一切路径（包括白名单外的）
    outside_path = Path(_EGENT_FILE)
    assert bad_rule.allows(outside_path), (
        "字符串 whitelist 导致的 '*' 意外匹配使白名单完全失效："
        f"'{_EGENT_FILE}' 本不应被允许但实际被允许"
    )


def test_whitelist_string_vs_tuple_allows_behavior():
    """对比 tuple whitelist 与 string whitelist 的 allows 行为差异。"""
    # 正确用法：tuple
    correct_rule = pv.PathPermissionRule(
        whitelist=(_AGENTS_GLOB,),
        blacklist=(),
    )

    # 错误用法：string（模拟 main.py 的 bug）
    buggy_rule = pv.PathPermissionRule(
        whitelist=_AGENTS_GLOB,
        blacklist=(),
    )

    # 测试白名单内文件：两者都应允许
    agents_path = Path(_AGENTS_FILE)
    assert correct_rule.allows(agents_path)
    assert buggy_rule.allows(agents_path)

    # 测试白名单外文件：正确规则应拒绝，buggy 规则因 '*' 意外匹配而允许
    outside_path = Path(_EGENT_FILE)
    assert not correct_rule.allows(outside_path), (
        "正确规则应拒绝白名单外的文件"
    )
    assert buggy_rule.allows(outside_path), (
        "buggy 规则因字符串逐字符迭代末尾 '*' 匹配一切，应（错误地）允许白名单外文件"
    )

    # 测试任意随机路径：buggy 规则也会允许（因为 '*' 匹配一切）
    random_path = Path("C:/Windows/system32/drivers/etc/hosts")
    assert not correct_rule.allows(random_path), (
        "正确规则应拒绝任意路径"
    )
    assert buggy_rule.allows(random_path), (
        "buggy 规则因 '*' 应（错误地）允许任意路径"
    )
