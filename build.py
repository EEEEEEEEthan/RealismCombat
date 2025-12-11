#!/usr/bin/env python3
"""
Godot 项目打包脚本
1. 导出项目到 .export 文件夹
2. 压缩导出文件，以时间命名，放到 .archive/
"""

import subprocess
import shutil
import os
from datetime import datetime
from pathlib import Path


# 配置
PROJECT_DIR = Path(__file__).parent
EXPORT_DIR = PROJECT_DIR / ".export"
ARCHIVE_DIR = PROJECT_DIR / ".archive"
EXPORT_PRESET = "Windows Desktop"
EXPORT_FILENAME = "RealismCombat.exe"
LOCAL_SETTINGS_FILE = PROJECT_DIR / ".local.settings"


def load_godot_path():
    """从 .local.settings 读取 Godot 路径"""
    if not LOCAL_SETTINGS_FILE.exists():
        print(f"警告: {LOCAL_SETTINGS_FILE} 不存在，使用默认 godot 命令")
        return "godot"
    
    with open(LOCAL_SETTINGS_FILE, 'r', encoding='utf-8') as f:
        for line in f:
            line = line.strip()
            if line.startswith("godot"):
                # 解析 "godot = path" 格式
                parts = line.split("=", 1)
                if len(parts) == 2:
                    return parts[1].strip()
    
    print("警告: .local.settings 中未找到 godot 路径，使用默认 godot 命令")
    return "godot"


def clean_export_dir():
    """清理导出目录"""
    if EXPORT_DIR.exists():
        print(f"清理导出目录: {EXPORT_DIR}")
        shutil.rmtree(EXPORT_DIR)
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)


def ensure_archive_dir():
    """确保存档目录存在"""
    ARCHIVE_DIR.mkdir(parents=True, exist_ok=True)


def export_project(godot_path):
    """导出 Godot 项目"""
    export_path = EXPORT_DIR / EXPORT_FILENAME
    
    print(f"正在导出项目到: {export_path}")
    
    cmd = [
        godot_path,
        "--headless",
        "--export-release",
        EXPORT_PRESET,
        str(export_path)
    ]
    
    result = subprocess.run(
        cmd,
        cwd=PROJECT_DIR,
        capture_output=True,
        text=True
    )
    
    if result.returncode != 0:
        print(f"导出失败!")
        print(f"stdout: {result.stdout}")
        print(f"stderr: {result.stderr}")
        return False
    
    print("导出成功!")
    return True


def create_archive():
    """压缩导出文件"""
    ensure_archive_dir()
    
    # 以时间命名
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    archive_name = f"RealismCombat_{timestamp}"
    archive_path = ARCHIVE_DIR / archive_name
    
    print(f"正在压缩到: {archive_path}.zip")
    
    # 创建 zip 压缩包
    shutil.make_archive(
        str(archive_path),
        'zip',
        EXPORT_DIR
    )
    
    final_path = f"{archive_path}.zip"
    file_size = os.path.getsize(final_path) / (1024 * 1024)  # MB
    
    print(f"压缩完成: {final_path} ({file_size:.2f} MB)")
    return final_path


def main():
    print("=" * 50)
    print("Godot 项目打包脚本")
    print("=" * 50)
    
    # 0. 加载 Godot 路径
    godot_path = load_godot_path()
    print(f"Godot 路径: {godot_path}")
    
    # 1. 清理并创建导出目录
    clean_export_dir()
    
    # 2. 导出项目
    if not export_project(godot_path):
        print("打包失败!")
        return 1
    
    # 3. 压缩
    archive_path = create_archive()
    
    print("=" * 50)
    print("打包完成!")
    print(f"导出目录: {EXPORT_DIR}")
    print(f"压缩文件: {archive_path}")
    print("=" * 50)
    
    return 0


if __name__ == "__main__":
    exit(main())
