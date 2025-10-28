# ResourceTable导出工具

## 功能
自动遍历 `Scenes` 目录下的所有 `.tscn` 文件，并将它们作为常量导出到 `Scripts/ResourceTable.cs` 的 `autogen_scenes` 区域。

## 使用流程

### 1. 启用插件
在Godot编辑器中：
- 点击菜单 `项目` -> `项目设置` -> `插件`
- 找到 `ResourceTableExporter` 
- 勾选启用

### 2. 生成常量
启用后，在编辑器顶部工具栏会出现一个按钮：**"生成ResourceTable"**

点击这个按钮即可自动生成所有场景资源的常量。

### 3. 查看结果
生成完成后，打开 `Scripts/ResourceTable.cs`，在 `#region autogen_scenes` 区域内会看到生成的常量。

## 命名规则

常量名称会根据场景文件的路径和文件名自动生成：

- `Scenes/Game.tscn` → `game`
- `Scenes/MainMenu.tscn` → `mainMenu`
- `Scenes/Components/PrinterLabelNode.tscn` → `componentsPrinterLabelNode`
- `Scenes/Dialogues/GenericDialogue.tscn` → `dialoguesGenericDialogue`

路径中的目录名和文件名会被转换为驼峰命名法（首字母小写）。

## 注意事项

- 不要手动修改 `#region autogen_scenes` 区域内的代码，每次生成都会覆盖
- 如果需要自定义常量名，请在该区域外手动添加

