"""egent 开发工作流：编码、验收循环（pytest 回归，无白盒测试）。"""

from __future__ import annotations

import subprocess
import sys
from pathlib import Path

import _common
import conversation_printer
import egent.agent
import egent.builtin_tools.path_validator

_EGENT_DIR = Path(__file__).resolve().parent
_PROJECT_ROOT = _EGENT_DIR.parent
_PYTEST_TIMEOUT_SECONDS = 120.0


class CodingGaveUp(Exception):
    """开发者主动放弃任务。"""

    def __init__(self, reason: str) -> None:
        self.reason = reason
        super().__init__(reason)


def _egent_coder_path_permissions(
    project_root: str,
) -> egent.builtin_tools.path_validator.PathPermissions:
    """egent 开发者路径权限：可读写 .egent，可读全项目。"""
    return egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
                f"{project_root}/.git",
                f"{project_root}/.godot",
                f"{project_root}/.export",
                f"{project_root}/.logs",
            ),
        ),
        readable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=("*/.model.toml",),
        ),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(
                f"{project_root}/.egent",
                f"{project_root}/.egent/*",
                f"{project_root}/pyproject.toml",
            ),
            blacklist=(
                "*/.model.toml",
                "*.pyc",
                "*/.pytest_cache/*",
                "*/.ruff_cache/*",
                "*/__pycache__/*",
                f"{project_root}/.egent/.model.toml",
            ),
        ),
    )


def _egent_reviewer_path_permissions(
    project_root: str,
) -> egent.builtin_tools.path_validator.PathPermissions:
    """egent 验收员路径权限：只读，可发现 .egent 变更。"""
    return egent.builtin_tools.path_validator.PathPermissions(
        discoverable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=(
                "*.pyc",
                "*/.pytest_cache",
                "*/.ruff_cache",
                "*/__pycache__",
                f"{project_root}/.git",
                f"{project_root}/.godot",
                f"{project_root}/.export",
                f"{project_root}/.logs",
            ),
        ),
        readable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(project_root, f"{project_root}/*"),
            blacklist=("*/.model.toml",),
        ),
        editable=egent.builtin_tools.path_validator.PathPermissionRule(
            whitelist=(),
            blacklist=(),
        ),
    )


def _run_pytest(spec: str = "") -> tuple[bool, str]:
    """运行 .egent/tests 下的 pytest，返回 (是否通过, 输出)。"""
    command = [sys.executable, "-m", "pytest"]
    normalized_spec = spec.strip()
    if normalized_spec and normalized_spec.lower() != "all":
        command.append(normalized_spec)
    try:
        test_result = subprocess.run(
            command,
            cwd=_PROJECT_ROOT,
            capture_output=True,
            text=True,
            check=False,
            timeout=_PYTEST_TIMEOUT_SECONDS,
        )
    except subprocess.TimeoutExpired:
        return False, f"pytest 超时（{_PYTEST_TIMEOUT_SECONDS:.0f}s）"
    output = f"{test_result.stdout}\n{test_result.stderr}".strip()
    if test_result.returncode == 0:
        return True, output or "测试通过"
    return False, output


async def coding(
    coder: egent.agent.Agent,
    prompt: str,
) -> tuple[bool, str]:
    """执行 egent 开发：实现、优化、跑 pytest；最多重试直至通过。"""
    project_root = Path.cwd().resolve().as_posix()
    coder.path_permissions = _egent_coder_path_permissions(project_root)

    def run_pytest_test(spec: str = "all") -> str:
        """运行 .egent 目录下的 pytest 测试并返回输出。

        @param spec: 测试范围：all（全部）、或 pytest 节点 id / 文件路径
        """
        _, output = _run_pytest(spec)
        return output

    coder.add_message(
        "system",
        f"{prompt}"
        "## 实现原则\n"
        "不要追求最小 diff，应追求最优雅、最易维护的实现。\n"
        "若现有结构阻碍正确性、可读性或可扩展性，主动重构相关代码；宁可多做一步，也不留补丁式或凑合式修改。"
        "编码完整后使用 run_pytest_test 进行你的专项测试（不需要全跑，跑你相关的专项测试即可；你提交之后会有专门的流程跑测试）。"
        "测试代码写在 .egent/tests/ 下；pylint 评分必须 10/10（tests 目录允许 duplicate-code ignore）。",
    )

    last_failure_output = ""
    for _ in range(5):
        coder.tools = [
            *_common.GIT_READ_ONLY_TOOLS,
            run_pytest_test,
        ]
        submitted = await coder.request_submit({
            "success": (bool, "true表示任务完成,false表示放弃"),
            "reason": (str, "如果放弃，填放弃原因,例如需求不合理,或者无法实现等。否则填一个减号`-`"),
        })

        if not submitted["success"]:
            raise CodingGaveUp(submitted["reason"])

        coder.add_message(
            "system",
            "编码已完成。请使用 code-optimize技能优化代码",
        )
        coder.tools = list(_common.GIT_READ_ONLY_TOOLS)
        await coder.request()

        passed, last_failure_output = _run_pytest()
        if passed:
            return True, ""

        coder.add_message(
            "system",
            f"pytest 回归测试失败。请修复:\n\n{last_failure_output}\n\n请仔细查看需求:\n\n{prompt}",
        )

    coder.add_message(
        "system",
        "pytest 回归测试连续失败。开发计划暂停。"
        "请总结本次开发工作遇到的问题。",
    )
    coder.tools = []
    await coder.request()
    return False, coder.last_message


async def review(prompt: str) -> tuple[bool, str]:
    """验收 egent 开发成果是否满足需求。"""
    reviewer = egent.agent.Agent(
        "gpt5",
        skills=_common.discover_project_skills(),
    )
    project_root = Path.cwd().resolve().as_posix()
    reviewer.path_permissions = _egent_reviewer_path_permissions(project_root)
    with conversation_printer.ConversationPrinter(reviewer):
        reviewer.add_message(
            "system",
            "你是这个项目的 egent 工作流验收员。你需要验收 .egent 目录下的开发成果是否满足需求。"
            "使用 git_diff 查看代码变更，结合当前项目结构和需求文档进行验收。"
            f"\n\n## 需求:\n{prompt}\n\n"
            "## 验收标准:\n"
            "验证变更是否符合需求\n"
            "验证 .egent/tests 下的 pytest 是否覆盖了本次修改\n"
            "验证 pylint 评分是否为 10/10\n"
            "验证实现是否追求最优雅解，而非最小改动；若仅为凑合可用、补丁堆砌或未做必要重构，应驳回\n"
            "根据 code-optimize 技能检查维护成本与结构质量\n\n"
            "验收通过或者拒绝,都要使用 submit_task 提交验收结果\n",
        )
        reviewer.tools = list(_common.GIT_READ_ONLY_TOOLS)
        submitted = await reviewer.request_submit({
            "is_accepted": (bool, "是否通过验收"),
            "summary": (str, "验收意见摘要"),
        })
    return submitted["is_accepted"], submitted["summary"]


async def begin_egent_develop_workflow(description: str) -> tuple[bool, str]:
    """运行 egent 开发工作流：编码、验收循环，直至通过或耗尽重试。"""
    developer = egent.agent.Agent(
        "gpt5-flash",
        skills=_common.discover_project_skills(),
    )
    printer = conversation_printer.ConversationPrinter(developer)
    developer.add_message("system", "你是这个项目的 egent 工作流开发工程师")
    developer.add_message(
        "system",
        "你收到了新的需求。请做完这个需求并更新 .egent/tests 下的 pytest 测试。"
        "如果任务无法完成,请说明原因并放弃任务。",
    )

    for _ in range(5):
        try:
            finished, coding_message = await coding(developer, description)
        except CodingGaveUp as error:
            return False, f"你的手下放弃了任务。原因是: \n{error.reason}"

        if not finished:
            developer.add_message("system", "你的工作无法顺利完成。请总结本次工作")
            await printer.request()
            return False, (
                "工作无法顺利完成\n\n"
                + developer.last_message
                + "\n\n---\npytest 回归测试:\n"
                + f"❌ 未通过（已重试 5 次）\n\n{coding_message}"
            )

        passed, accept_message = await review(description)
        if passed:
            developer.add_message(
                "system",
                "审查通过！请总结本次工作。",
            )
            await printer.request()
            return True, (
                "工作顺利完成\n\n"
                + developer.last_message
                + "\n\n---\n验收结果:\n"
                + f"✅ 验收通过\n\n{accept_message}\n\n当前状态:等待提交"
            )

        await developer.summarize()
        developer.add_message(
            "system",
            f"验收未通过，请根据验收意见修复:\n\n{accept_message}",
        )

    developer.add_message("system", "你的工作无法顺利完成。请总结本次工作")
    await printer.request()
    return False, "工作无法顺利完成\n\n" + developer.last_message
