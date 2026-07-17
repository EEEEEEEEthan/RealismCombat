"""将项目根目录加入 ``sys.path``，便于直接运行本目录下的脚本。"""

from __future__ import annotations

import sys
from pathlib import Path

_PROJECT_ROOT = Path(__file__).resolve().parent.parent
_PROJECT_ROOT_TEXT = str(_PROJECT_ROOT)

if _PROJECT_ROOT_TEXT not in sys.path:
    sys.path.insert(0, _PROJECT_ROOT_TEXT)
