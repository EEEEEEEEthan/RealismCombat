# build

运行打包脚本 `build.py` 进行项目导出和归档。

```bash
python build.py
```

脚本将自动：
1. 从 `.local.settings` 读取 Godot 路径
2. 清理并导出项目到 `.export/` 目录
3. 压缩导出文件到 `.archive/` 目录，以时间命名
