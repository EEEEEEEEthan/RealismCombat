# RealismCombat 项目文档

## 核心系统

### 资源管理

项目使用资源表系统来管理和加载游戏资源：

- `ResourceTable`：管理文件资源的加载，使用延迟加载机制
- `SpriteTable`：管理从纹理图集中提取的精灵资源
- `Cache<T>` 和 `Loader<T>`：提供延迟加载和缓存功能

**重要规则：**
- 所有游戏资源（纹理、音频、场景等）必须通过 `ResourceTable` 或 `SpriteTable` 加载
- 禁止在代码中直接使用 `GD.Load()` 加载资源
- 新增资源时，先在 `ResourceTable` 中添加对应的 `Loader` 定义
- 这样可以确保资源的统一管理和延迟加载

### 音频管理

`AudioManager` 提供统一的音频管理：
- 在 `ProgramRoot` 初始化时创建，挂载为 `/root/ProgramRoot/AudioManager`
- 使用两个 `AudioStreamPlayer` 分别管理背景音乐（BGM）和音效（SFX）
- 提供播放、停止、音量控制等功能
- 其他组件通过全局路径获取 `AudioManager` 实例来播放音频

**使用规则：**
- 所有音频资源必须在 `ResourceTable` 中定义
- 通过 `AudioManager` 的 `PlayBgm()` 和 `PlaySfx()` 方法播放音频
- 避免在各个组件中独立创建 `AudioStreamPlayer`

### UI 系统

#### 文字打印机 (Printer)

`Printer` 组件继承自 `RichTextLabel`，提供逐字打印效果：
- 支持可配置的打印间隔
- 提供 `Printing` 属性用于查询是否正在打印
- 打字音效通过 `AudioManager` 播放

#### 通用对话框 (GenericDialogue)

`GenericDialogue` 组件提供通用的对话框功能：
- 支持多段文本的连续显示
- 使用 `Printer` 组件实现打字机效果
- 显示向下箭头图标指示玩家可以继续
- 玩家按键后显示下一段文本
- 长按任意键可加速文本显示

## MCP 服务器

详见 [mcp-server.md](mcp-server.md)

