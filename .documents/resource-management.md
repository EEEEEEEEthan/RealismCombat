# 资源管理

项目通过资源表集中管理所有二进制与场景资源，确保加载路径、缓存策略与 Godot 引擎保持一致性。

## Cache 与 Loader

- `Cache<T>` 接受一个工厂方法，首次访问 `Value` 时调用工厂并缓存结果，之后直接返回缓存
- 提供 `implicit operator T`，可直接将 `Cache<T>` 当作资源实例使用
- `Loader<T>` 继承自 `Cache<T>`，内部工厂实现 `GD.Load<T>(path)`，约束 `T` 为引用类型
- 任何需要自定义创建逻辑（例如从图集裁剪）都可以使用 `new Cache<T>(() => ...)` 完成
- 资源创建失败会在首次访问时抛出异常，因此建议提前通过单元流程或场景加载自测

## ResourceTable

- 集中声明所有外部资源路径，便于后续统一重构或替换
- 当前表项按类型归档：
  - 纹理：`icon8`、`bleeding`（流血动画图集）
  - 音频：`typingSound`、`arpegio01Loop`、`battleMusic1`、`oneBeep`、`retroClick`、`retroHurt1`、`selection3`、`blockSound`
  - 场景：`gameNodeScene`、`combatNodeScene`、`characterNodeScene`、`propertyNodeScene`、`menuDialogueScene`
- 访问方式示例：

```csharp
AudioManager.PlaySfx(ResourceTable.retroHurt1, -6);
PackedScene combatScene = ResourceTable.combatNodeScene;
```

- 新增资源步骤：
  - 将资源文件放入对应目录（如 `res://Textures/`）
  - 在 `ResourceTable` 添加 `Loader<T>` 字段，命名遵循驼峰式含义明确的规则
  - 在代码中引用字段，不直接写入字符串路径

## SpriteTable
- `SpriteTable` 基于 `Cache<AtlasTexture>` 将图集中子区域切割成可直接使用的精灵
- 当前条目：
  - `ArrowRight`：菜单列表指示箭头，供 `MenuOptionList` 与菜单类 UI 使用
  - `ArrowDown`：通用对话框快进箭头，供 `GenericDialogue` 闪烁提示
  - `Star`：通用星标小图标
  - `Bleeding`：从 `ResourceTable.bleeding` 切出的 11 帧动画，供 `BleedingNode` 使用
- `CreateAtlas()` 会生成新的 `AtlasTexture`，设置 `Atlas`、`Region` 并返回
- 适用于 UI 指示器、图标等轻量资源，避免重复手写裁剪逻辑
- 新增条目时需关注图集坐标与宽高，确保与原始像素对齐

## 场景资源

- 场景文件通过 `ResourceTable` 统一加载，以便集中治理依赖
- 常见用法：
  - `GameNode.tscn`：承载游戏循环
  - `CombatNode.tscn`：战斗 UI
  - `CharacterNode.tscn`：角色数据展示
- 使用 `Instantiate<T>()` 获取强类型节点，减少类型转换

## 音频资源

- 所有音频流均在 `ResourceTable` 注册，由 `AudioManager` 或特定节点使用
- `Printer` 在 `AudioManager` 尚未加载时会自动创建本地 `AudioStreamPlayer`，同样复用 `ResourceTable.typingSound`
- 在播放音效时注意指定基础音量，`AudioManager` 会在最终播放时叠加全局音量偏移

## 资源管理规范

- 禁止在业务逻辑中直接使用 `GD.Load()` 或硬编码资源路径
- 若资源需要在多个模块共享，应优先放入 `ResourceTable`，并根据用途命名
- 对于体积较大的资源可借助 `Cache<T>` 实现懒加载，确保仅在真正需要时才触发 IO
- 场景实例化后应在不需要时调用 `QueueFree()`，避免泄漏与重复占用内存

