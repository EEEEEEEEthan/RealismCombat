# 编辑器工具

## RebuildWithTabs 插件
- 位置：`res://addons/RebuildWithTabs/`
- 功能：在工具栏提供 `Rebuild` 按钮，用于在重编译 C# 项目前后保持场景标签页状态。
- 行为流程：
  1. 记录当前打开的场景列表，并获取当下激活的场景标签页。
  2. 关闭所有场景标签页（优先通过 `SceneTabs`，找不到则逐个 `close_scene()` 兜底）。
  3. 通过 `OS.execute("dotnet", ["build", "<project>/RealismCombat.csproj", "--nologo"], ...)` 构建 C# 项目，成功后调用 `get_resource_filesystem().scan()` 刷新资源。
  4. 依次恢复之前的场景标签页，并将关闭前的激活场景设为当前标签页。
- 适用范围：只处理场景标签页（`.tscn`），不处理脚本标签页。
- 注意：
  - 若场景文件路径失效，将跳过并输出提示。
  - 关闭标签页和重编译之间存在少量延迟（逐帧等待）以确保操作顺序。

