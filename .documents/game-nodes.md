# 游戏节点系统

## 概述

游戏节点系统包含所有用于游戏逻辑和 UI 展示的 Godot 节点类，位于 `Scripts/Nodes/` 目录下。这些节点负责战斗界面、角色展示、属性显示等核心功能。

## 节点分类

### 游戏核心节点

#### ProgramRoot

程序根节点，位于 `Scripts/Nodes/ProgramRoot.cs`，负责初始化游戏生命周期。

##### 功能

- 程序启动入口，在 `_Ready()` 中完成基础初始化
- 处理 MCP 模式检测，若 `LaunchArgs.port` 存在则挂载 `CommandHandler`
- 维护主菜单循环（`StartGameLoop()`），使用永真循环保证异常不会中断主流程
- 管理存档槽位系统，支持 7 个存档槽位
- 提供存档选择流程（`SelectSaveSlot()`），支持新建和读取两种模式

##### 存档系统

- `GetSaveFilePath(int slotIndex)`：获取存档文件路径，格式为 `user://save_slot_{slotIndex + 1}.sav`
- `CreateSaveSlotOption(int slotIndex)`：创建槽位菜单项，显示存档标题和描述
- `SelectSaveSlot(bool requireExisting)`：选择存档槽位，支持返回选项
- 新建存档时，如果槽位已有存档，会提示是否覆盖
- 读取存档时，如果槽位为空，会提示并重新选择

##### 主循环

- `StartGameLoop()`：异步方法，维护应用生命周期
- `Routine()`：主菜单循环，提供“开始游戏”“读取游戏”“退出游戏”三项
- `RunNewGame(string saveFilePath)`：运行新游戏，实例化 `GameNode` 并创建新的 `Game` 对象
- `RunLoadedGame(string saveFilePath)`：读取存档并运行游戏，从文件反序列化 `Game.Snapshot`

##### 使用示例

```csharp
// ProgramRoot 在场景树中自动初始化
// 如果以 MCP 模式启动，会自动挂载 CommandHandler
// 否则直接调用 StartGameLoop() 进入主菜单
```

#### GameNode

游戏节点，位于 `Scripts/Nodes/Games/GameNode.cs`，作为 `Game` 的宿主节点挂载到场景树。

##### 功能

- 作为 `Game` 的宿主节点，承载游戏逻辑
- 主要用于承载后续的战斗界面或其它子系统节点
- 当前实现为空，预留接口供后续扩展

##### 使用示例

```csharp
// 在 ProgramRoot 中实例化
PackedScene gameNodeScene = ResourceTable.gameNodeScene;
var gameNode = gameNodeScene.Instantiate();
AddChild(gameNode);
var game = new Game(saveFilePath, gameNode);
await game;
// 游戏结束后释放节点
gameNode.QueueFree();
```

### 战斗界面节点

#### CombatNode

战斗节点，位于 `Scripts/Nodes/Games/CombatNode.cs`，提供战斗界面的 UI 框架。

##### 功能

- 管理战斗界面布局，包含玩家与敌人的角色栏容器
- `Initialize(Combat combat)`：初始化战斗，清空旧节点并创建新的 `CharacterNode`
- `GetCharacterNode(Character character)`：通过角色引用查找对应的 UI 节点
- 提供位置查询方法，用于战斗动画定位

##### 容器结构

- `PlayerTeamContainer`：玩家队伍容器（`VBoxContainer`）
- `EnemyTeamContainer`：敌人队伍容器（`VBoxContainer`）
- `PlayerPkPosition`：玩家对战位置（`Control`）
- `EnemyPkPosition`：敌人对战位置（`Control`）
- `PlayerReadyPosition`：玩家待命位置（`Control`）
- `EnemyReadyPosition`：敌人待命位置（`Control`）

##### 位置查询方法

- `GetPKPosition(Character character)`：获取角色对战位置
- `GetReadyPosition(Character character)`：获取角色待命位置
- `GetHitPosition(Character character)`：获取角色受击位置（对战位置偏移 12 像素）
- `GetDogePosition(Character character)`：获取角色闪避位置（对战位置偏移 -12 像素）

##### 使用示例

```csharp
var combatNode = ResourceTable.combatNodeScene.Instantiate<CombatNode>();
combatNode.Initialize(combat);
// 战斗中使用位置查询
var position = combatNode.GetPKPosition(character);
characterNode.MoveTo(position);
```

#### CharacterNode

角色节点，位于 `Scripts/Nodes/Games/CharacterNode.cs`，展示角色的详细信息。

##### 功能

- 展示角色名称、行动点、生命值等属性
- 支持展开/折叠状态，展开时显示详细的身体部位信息
- 提供动画接口：移动、摇晃、闪烁等
- 支持主题切换（玩家/敌人），背景颜色会根据主题和生命值变化

##### 属性显示

- `actionPointNode`：行动点显示（`PropertyNode`）
- `hitPointNode`：总生命值显示（`PropertyNode`），折叠时显示
- `headHitPointNode`：头部生命值（`PropertyNode`），展开时显示
- `leftArmHitPointNode`、`rightArmHitPointNode`：双臂生命值
- `torsoHitPointNode`：躯干生命值
- `leftLegHitPointNode`、`rightLegHitPointNode`：双腿生命值
- `reactionContainer`：反应点数显示容器

##### 展开/折叠

- `Expanded`：展开状态属性（`bool`）
- `minSize`：折叠时最小尺寸（默认 `55×39`）
- `maxSize`：展开时最大尺寸（默认 `55×86`）
- `ExpandScope()`：返回 `IDisposable`，在作用域内自动展开，离开时自动折叠

##### 动画方法

- `MoveTo(Vector2 globalPosition)`：平滑移动到指定全局坐标，使用 `Tween` 实现，持续 0.2 秒
- `Shake()`：产生一次横向晃动并回到原位，使用 `Tween` 实现，持续约 0.06 秒
- `MoveScope(Vector2 globalPosition)`：返回 `IDisposable`，在作用域内自动移动，离开时回到原位

##### 主题系统

- `IsEnemyTheme`：是否为敌人主题（`bool`）
- 玩家主题使用 `skyBlueGradient`，敌人主题使用 `sunFlareOrangeGradient`
- 背景颜色根据生命值比例变化：
  - `> 0.3`：使用梯度第 2 个颜色
  - `> 0.25`：使用梯度第 3 个颜色
  - `≤ 0.25`：使用梯度第 4 个颜色
- 角色死亡时，背景颜色变为灰色（`grayGradient[^2]`）

##### 状态同步

- 在 `_Process()` 中同步角色状态：
  - 行动点：如果有战斗行为或正在考虑，会显示跳动效果
  - 生命值：总生命值取头部和躯干中较小比例者
  - 反应点数：根据角色 `reaction` 属性更新显示

##### 受击反馈

- `FlashPropertyNode(ICombatTarget combatTarget)`：根据战斗目标闪烁对应的属性节点
- 支持身体部位和装备两种战斗目标类型

##### 使用示例

```csharp
var characterNode = ResourceTable.characterNodeScene.Instantiate<CharacterNode>();
characterNode.IsEnemyTheme = true;
characterNode.Initialize(combat, character);
// 展开角色信息
using (characterNode.ExpandScope())
{
    // 显示详细身体部位信息
}
// 移动到对战位置
characterNode.MoveTo(combatNode.GetPKPosition(character));
// 受击时摇晃
characterNode.Shake();
```

#### CardFrame

卡片框节点，位于 `Scripts/Nodes/CardFrame.cs`，为角色卡片提供背景底色、流血遮罩与闪光动画。

##### 功能

- 作为 `CharacterNode` 的外层容器，承载尺寸变更、抖动与闪光等视觉效果
- `Color` 导出属性驱动背景颜色，通过内部 `%Background` 的 `SelfModulate` 应用
- `Bleeding` 导出属性控制 `%Bleeding` 图层显隐，用于展示流血状态

##### 节点结构

- `%Background`：卡片背景
- `%Bleeding`：流血遮罩层
- `%Flash/FlashContent`：闪光特效节点，水平扫过卡片

##### 闪光效果

- `Flash()` 会终止旧的 tween，重新计算起止位置并在 0.2 秒内让 `FlashContent` 从左至右扫过
- `CharacterNode.FlashFrame()` 在受击时调用，用于提供即时反馈

##### 使用示例

```csharp
var cardFrame = GetNode<CardFrame>("%CardFrame");
cardFrame.Color = GameColors.skyBlueGradient[1];
cardFrame.Bleeding = character.IsBleeding;
cardFrame.Flash();
```

#### PropertyNode

属性节点，位于 `Scripts/Nodes/Games/PropertyNode.cs`，用于显示属性条。

##### 功能

- 显示属性标题、当前值与最大值
- 支持跳动效果（`Jump` 属性），使用 Shader 实现
- 支持闪烁效果（`FlashRed()`），用于受击反馈

##### 属性

- `Title`：属性标题（`string`）
- `Value`：属性值元组（`(double current, double max)`）
- `Current`：当前值（`double`）
- `Max`：最大值（`double`）
- `Progress`：进度比例（`double`），计算为 `Current / Max`
- `Jump`：是否启用跳动效果（`bool`）

##### 跳动效果

- 使用内置 Shader 实现像素级跳动效果
- Shader 源码内嵌于脚本，运行时动态创建共享材质实例
- 通过 `fract_random()` 函数实现伪随机像素偏移
- 跳动间隔为 0.15 秒

##### 闪烁效果

- `FlashRed()`：闪烁红色，持续 0.2 秒
- 使用 `pinkGradient[^1]`（深红色 `#b21030`）
- 闪烁后自动恢复原颜色

##### 使用示例

```csharp
var propertyNode = ResourceTable.propertyNodeScene.Instantiate<PropertyNode>();
propertyNode.Title = "行动";
propertyNode.Value = (5, 10);
propertyNode.Jump = true; // 启用跳动效果
// 受击时闪烁
propertyNode.FlashRed();
```

#### PropertyNode2

属性节点2，位于 `Scripts/Nodes/Games/PropertyNode2.cs`，用于显示属性条，是 `PropertyNode` 的简化版本。

##### 功能

- 显示属性标题、当前值与最大值
- 显示整数格式的数值（`当前值/最大值`）
- 支持闪烁效果（`FlashRed()`），用于受击反馈
- **不支持**跳动效果（`Jump` 功能）

##### 属性

- `Title`：属性标题（`string`）
- `Value`：属性值元组（`(int current, int max)`），返回整数类型
- `Current`：当前值（`double`）
- `Max`：最大值（`double`）
- `Progress`：进度比例（`double`），计算为 `Current / Max`

##### 数值显示

- 在进度条上方显示整数格式的数值
- 格式：`当前值/最大值`（例如 `5/10`）
- 数值会随 `Current` 和 `Max` 自动更新

##### 闪烁效果

- `FlashRed()`：闪烁红色，持续 0.2 秒
- 使用 `pinkGradient[^1]`（深红色 `#b21030`）
- 闪烁后自动恢复原颜色

##### 与 PropertyNode 的区别

- `PropertyNode2` 不支持 `Jump` 跳动效果
- `PropertyNode2.Value` 返回 `(int, int)` 而非 `(double, double)`
- `PropertyNode2` 在进度条上方显示数值文本

##### 使用示例

```csharp
var propertyNode2 = ResourceTable.propertyNode2Scene.Instantiate<PropertyNode2>();
propertyNode2.Title = "生命";
propertyNode2.Value = (50, 100); // 整数类型
// 受击时闪烁
propertyNode2.FlashRed();
```

#### BleedingNode

流血节点，位于 `Scripts/Nodes/Games/BleedingNode.cs`，提供流血动画效果。

##### 功能

- 显示角色受伤时的流血动画
- 使用 `SpriteTable.Bleeding` 中的动画帧，共 11 帧
- 在 `_Process()` 中根据随机间隔切换动画帧，实现不规则闪烁效果

##### 动画机制

- 时间间隔在 0 到 0.3 秒之间随机（`GD.Randf() * 0.3`）
- 动画帧循环播放，使用模运算（`index % SpriteTable.Bleeding.Count`）
- 当节点可见性改变时，会重置动画索引和时间，确保动画连贯

##### 使用示例

```csharp
// BleedingNode 通常作为 CharacterNode 的子节点
// 在角色受击时显示，表示受伤状态
var bleedingNode = new BleedingNode();
characterNode.AddChild(bleedingNode);
```

### 对话框节点

对话框节点系统已在 [UI 系统文档](ui-system.md) 中详细说明，包括：

- `BaseDialogue`：对话框基类
- `GenericDialogue`：通用对话框
- `MenuDialogue`：菜单对话框
- `Printer`：文字打印机

## 节点设计原则

### 代码驱动

- 所有节点结构均在代码中创建，不依赖场景文件
- 使用 `[Tool, GlobalClass]` 属性，使组件能够在编辑器和运行时复用
- 节点层级、主题、样式全部以代码维护，保持版本控制可追踪性

### 作用域模式

- `CharacterNode` 提供 `ExpandScope()` 和 `MoveScope()` 等方法
- 使用 `IDisposable` 模式实现自动状态恢复
- 确保动画状态在作用域结束时自动清理

### 主题系统

- 通过 `IsEnemyTheme` 切换玩家/敌人主题
- 颜色使用 `GameColors` 中定义的梯度
- 背景颜色根据生命值比例动态变化

### 状态同步

- 关键节点在 `_Process()` 中同步游戏状态
- 确保 UI 始终反映最新的游戏数据
- 使用属性绑定减少手动同步代码

## 节点生命周期

### 创建与初始化

1. 从 `ResourceTable` 获取场景资源
2. 实例化节点并添加到场景树
3. 调用 `Initialize()` 方法初始化状态
4. 在 `_Ready()` 中完成节点结构搭建

### 使用阶段

- 节点在游戏循环中使用
- 状态在 `_Process()` 中同步
- 动画通过 `Tween` 实现平滑过渡

### 释放

- 游戏结束时调用 `QueueFree()` 释放节点
- 确保所有资源正确清理
- 避免内存泄漏和资源残留

## 扩展指南

### 添加新节点

1. 在 `Scripts/Nodes/` 目录下创建新的节点类
2. 继承适当的基类（`Node`、`Control` 等）
3. 实现必要的生命周期方法（`_Ready()`、`_Process()` 等）
4. 如需场景资源，在 `ResourceTable` 中注册

### 扩展现有节点

- 在现有节点类中添加新方法
- 使用 `[Export]` 属性暴露可配置参数
- 保持与现有设计风格一致

### 添加新动画

- 使用 Godot 的 `Tween` 系统实现动画
- 遵循现有的动画持续时间约定（0.2 秒）
- 考虑提供作用域模式的便捷方法

## 注意事项

- 所有节点创建和修改都应在主线程中进行
- 使用 `CallDeferred()` 确保节点准备好后再操作
- 注意节点的生命周期，避免访问已释放的节点
- 动画使用 `Tween` 时，注意在开始新动画前取消旧动画（`tween?.Kill()`）

