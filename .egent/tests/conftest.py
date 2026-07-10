"""pytest 公共配置：将 .egent 目录加入模块搜索路径。"""

from __future__ import annotations

import sys
from pathlib import Path

_EGENT_DIR = Path(__file__).resolve().parent.parent
if str(_EGENT_DIR) not in sys.path:
    sys.path.insert(0, str(_EGENT_DIR))
