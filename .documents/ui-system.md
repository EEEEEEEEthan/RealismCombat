# UI 系统

## 设计原则

- 所有 UI 组件均在构造函数中通过代码创建节点结构，不依赖场景文件，方便重构与测试
- 使用 `[Tool, GlobalClass]` 属性，使组件能够在编辑器和运行时复用
- 节点层级、主题、样式全部以代码维护，保持版本控制可追踪性

## 对话框管理 (DialogueManager)

- Godot AutoLoad 单例，位于 `Scripts/AutoLoad/`
- `_Ready()` 时记录初始化日志，方便确认加载顺序
- 始终只维护一个活动对话框；若已有对话框仍存在，创建新对话框会抛出异常
- `_Input()` 统一接收输入事件，并将事件传递给当前对话框的 `HandleInput()` 实现
- 提供工厂方法：
  - `CreateGenericDialogue(params string[])`
  - `CreateMenuDialogue(params MenuOption[])`
  - `CreateMenuDialogue(bool allowEscapeReturn, params MenuOption[])`
- `GetTopDialogue()` 与 `GetDialogueCount()` 可用来查询当前栈状态，供调试或自动化使用

### 菜单选项结构 (MenuOption)

- `MenuOption` 是一个结构体，包含 `title` 与 `description`
- `MenuDialogue` 使用标题渲染选项列表，同时将描述交给 `Printer` 显示详细信息
- 允许按需构造数组或使用集合，便于动态菜单

### 对话框基类 (BaseDialogue)

- 继承自 `PanelContainer`，构造函数中设置锚点与最小尺寸，统一摆放在屏幕底部
- 通过事件 `OnClosed` 通知 `DialogueManager` 当前对话框已关闭，避免重复释放
- `Close()` 负责触发事件并调用 `QueueFree()`；子类不得直接释放节点，必须走该方法
- 实现 `DialogueManager.IDialogue` 接口，将输入处理抽象为 `HandleInput()`

## 文字打印机 (Printer)

- 继承 `RichTextLabel`，提供逐字打印效果，默认间隔 `0.1f`
- `Printing` 属性指示是否仍在打印中，可用于控制动画或音效
- `interval` 与 `enableTypingSound` 以 `[Export]` 暴露，可在编辑器调试
- 当 `AudioManager` 尚未初始化时，会创建内部 `AudioStreamPlayer` 播放 `ResourceTable.typingSound`
- 每帧根据 `interval` 自动递增 `VisibleCharacters`，输入长按时可将间隔降为零实现快进

## 通用对话框 (GenericDialogue)

- 使用 `Printer` 加一个向下箭头图标构成 UI
- `AddText()` 支持逐段追加文本，新文本会追加至现有内容末尾
- `PrintDone` 任务在一组文本打印完毕后完成，可与战斗动画同步
- `TryNext()` 按顺序推进每一段文本
- 长按按键时会将 `Printer.interval` 设为 `0`，实现快速跳过
- 当 `LaunchArgs.port` 存在时会自动跳过玩家输入，方便自动化测试持续推进

## 菜单对话框 (MenuDialogue)

- 构造函数搭建选项列表、描述区域与指示箭头
- 支持 `allowEscapeReturn`，为真时自动追加“返回”选项并响应 `ui_cancel`
- 通过 `TaskCompletionSource<int>` 实现等待器，`await menu` 可直接获取选中的索引
- `Select(int index)` 会更新指示箭头位置，同时将对应描述传给 `Printer`
- `SelectAndConfirm(int index)` 用于 MCP 指令快速选择并确认
- 自动调用 `GameServer.McpCheckpoint()` 提示外部当前等待状态

## 异步等待机制

- 对话框通过实现自定义等待器，允许直接 `await dialogue`
- `MenuDialogue`、`GenericDialogue` 都会在完成后重置内部状态，允许重复使用
- `Game`、`Combat` 等业务流程围绕 `await` 语法构建，从而保持代码线性可读
- `DialogueManager` 与 `GameServer` 协作，使玩家输入与 MCP 自动化共用同一等待逻辑

