"""全项目 pylint 评分门禁。"""

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path

_EGENT_DIR = Path(__file__).resolve().parents[1]
_PROJECT_ROOT = _EGENT_DIR.parent
_PRODUCTION_TARGETS = tuple(
    str(path) for path in sorted(_EGENT_DIR.glob("*.py"))
)
_TEST_TARGETS = (str(_EGENT_DIR / "tests"),)
_SCORE_PATTERN = re.compile(r"rated at ([\d.]+)/10")


def _assert_pylint_perfect(targets: tuple[str, ...], *extra_args: str) -> None:
    completed = subprocess.run(
        [sys.executable, "-m", "pylint", *extra_args, *targets],
        cwd=_PROJECT_ROOT,
        capture_output=True,
        text=True,
        check=False,
    )
    output = f"{completed.stdout}\n{completed.stderr}"
    match = _SCORE_PATTERN.search(output)
    assert match is not None, output
    score = float(match.group(1))
    assert score == 10.0, output
    assert completed.returncode == 0, output


def test_egent_pylint_score_is_perfect() -> None:
    """``.egent`` 生产代码与测试的 pylint 评分必须为 10/10（duplicate-code 已在 pyproject 忽略）。"""
    _assert_pylint_perfect(_PRODUCTION_TARGETS)
    _assert_pylint_perfect(_TEST_TARGETS)
