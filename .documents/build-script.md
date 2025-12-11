# 打包脚本

项目提供了 Python 打包脚本 `build.py`，用于导出和归档 Godot 项目。

## 使用方法

```bash
python build.py
```

## 功能

1. **导出项目** - 使用 Godot 无头模式导出到 `.export/` 目录
2. **压缩归档** - 将导出文件压缩为 zip，保存到 `.archive/` 目录，文件名格式为 `RealismCombat_YYYYMMDD_HHMMSS.zip`

## 配置

脚本从 `.local.settings` 文件读取 Godot 路径：

```ini
godot = C:\Program Files\Godot\Godot_v4.6-dev4_mono_win64\Godot_v4.6-dev4_mono_win64.exe
```

如果该文件不存在或未配置，脚本会尝试使用系统 PATH 中的 `godot` 命令。

## 目录说明

| 目录 | 说明 |
|------|------|
| `.export/` | 导出输出目录，每次打包前会清空 |
| `.archive/` | 压缩包归档目录 |

这两个目录已添加到 `.gitignore`，不会被版本控制。
