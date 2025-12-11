# UI 系统

## 设计原则
 
- UI 组件的节点树通过 `.tscn` 维护，脚本仅负责行为与绑定，便于在编辑器中可视化调整
- 使用 `[Tool]` 属性以便编辑器可见；不再依赖 `GlobalClass` 暴露为全局类
- 通过 `ResourceTable` 统一加载 `PackedScene`，并提供静态 `Create(...)` 工厂创建实例

## 背景展示

- 需要提供背景图片的显示与隐藏系统，支持在不同场景切换时控制背景显隐

## 对话框管理 (DialogueManager)

- Godot AutoLoad 单例，位于 `Scripts/AutoLoad/`
- `_Ready()` 时记录初始化日志，方便确认加载顺序
- 始终只维护一个活动对话框；若已有对话框仍存在，创建新对话框会抛出异常
- `_Input()` 统一接收输入事件，并将事件传递给当前对话框的 `HandleInput()` 实现
- 提供工厂方法：
  - `CreateGenericDialogue()` - 创建通用对话框（无参构造）
  - `ShowGenericDialogue(string text, params string[] options)` - 便捷方法，显示文本后自动销毁
  - `ShowGenericDialogue(IEnumerable<string> texts)` - 便捷方法，显示多段文本后自动销毁
  - `DestroyDialogue(BaseDialogue dialogue)` - 手动销毁对话框
  - `CreateMenuDialogue(params MenuOption[])`
  - `CreateMenuDialogue(bool allowEscapeReturn, params MenuOption[])`
- `GetTopDialogue()` 与 `GetDialogueCount()` 可用来查询当前栈状态，供调试或自动化使用

### 菜单选项结构 (MenuOption)

- `MenuOption` 是一个结构体，包含 `title`、`description` 与 `disabled`
- `MenuDialogue` 使用标题渲染选项列表，同时将描述交给 `Printer` 显示详细信息
- `disabled` 字段用于标记选项是否禁用，禁用的选项会显示为灰色 (178,178,178) 且无法被选中或确认
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
- 无参构造，通过 `DialogueManager.CreateGenericDialogue()` 创建
- `ShowTextTask(string text, params string[] options)` - 核心 API，追加文本并返回任务
  - 无选项时：文本打印完成后显示向下箭头闪烁提示，按任意键继续，返回 `-1`
  - 有选项时：文本打印完成后显示选项（靠右对齐，间距8像素），使用左右/上下键切换，回车确认，返回选中索引
  - 选项在文本全部打印完成后才显示，确保用户先看到完整文本
- 支持在同一对话框实例中多次调用 `ShowTextTask()` 追加多段文本
- 长按按键时会将 `Printer.interval` 设为 `0`，实现快速跳过
- 交互音效：选项移动播放 `selection3`，确认播放 `oneBeep`
- 对话框不会自动销毁，需要外部调用 `DialogueManager.DestroyDialogue()` 或使用便捷方法 `ShowGenericDialogue()`
- 当 `LaunchArgs.port` 存在时：
  - 文本无选项会自动完成等待，便于自动化推进
  - 选项展示后会输出“请选择(game_select_option)”并调用 `GameServer.McpCheckpoint()`，可用 MCP 指令 `game_select_option` 远程选择并确认

## 菜单对话框 (MenuDialogue)

- 使用 `Scenes/MenuDialogue.tscn` 定义节点树：`PanelContainer/MarginContainer/HBoxContainer/{VBoxContainer, Printer}` 与 `Control/Indexer/TextureRect`
- 通过 `ResourceTable.menuDialogueScene` 加载，提供 `MenuDialogue.Create(options, allowEscapeReturn)` 创建实例
- 脚本在 `_Ready()` 中通过属性惰性绑定节点引用，并设置索引箭头纹理
- 支持 `allowEscapeReturn`，为真时自动追加"返回"选项并响应 `ui_cancel`
- 通过 `TaskCompletionSource<int>` 实现等待器，`await menu` 可直接获取选中的索引
- `Select(int index)` 会更新指示箭头位置，同时将对应描述传给 `Printer`；描述文本不再使用打字机效果，切换选项时即时显示完整内容；如果选项被禁用则不会选择
- `SelectAndConfirm(int index)` 用于 MCP 指令快速选择并确认
- 自动调用 `GameServer.McpCheckpoint()` 提示外部当前等待状态
- 禁用选项处理：
  - 在 `BuildOptions()` 中，禁用的选项会设置 Label 的 `Modulate` 为灰色 (178,178,178)
  - 导航时（`ui_up`/`ui_down`）会自动跳过禁用的选项
  - 确认时（`ui_accept`）如果当前选项被禁用则不会确认
  - 初始化时会自动选择第一个可用的选项

### 菜单选项列表 (MenuOptionList)

- 位置：`Scripts/Nodes/Dialogues/MenuOptionList.cs`，`[Tool][GlobalClass]`，继承 `MarginContainer`
- 提供 8 行可视窗口（`VisibleLines`），超出部分使用省略行 `...(+N)` 展示剩余数量，上滚与下滚各占一行
- 通过导出属性 `Options` 绑定 `MenuOptionResource` 数组，支持编辑器直接填充；`Index` 控制当前选中项并自动校正可视窗口
- 禁用项使用统一灰色 (`Color(178/255,178/255,178/255)`)，文本取 `MenuOptionResource.text`
- 指示箭头使用 `SpriteTable.ArrowRight`，挂载在内部 `indicatorHost` 上，选中项时同步到对应 Label 的全局位置
- `_Ready()` 中延迟调用 `Rebuild()`，确保节点完全准备后再渲染列表，避免编辑器和运行时出现空引用
- `MenuDialogue` 的 `BuildOptions()` 会将 `MenuOption` 转换为 `MenuOptionResource`，并驱动 `Index` 以便与描述文本同步

#### 数据资源 (MenuOptionResource)

- 位置：`Scripts/Nodes/Dialogues/MenuOptionResource.cs`，`[Tool][GlobalClass]`，继承 `Resource`
- 导出字段：
  - `text`：选项展示文本
  - `disabled`：是否禁用，用于渲染禁用态且阻止确认
- 主要由 `MenuOptionList` 读取，也可在编辑器直接创建数组驱动长列表

## 异步等待机制

- `MenuDialogue` 通过实现自定义等待器，允许直接 `await menu` 获取选中索引
- `GenericDialogue` 通过 `ShowTextTask()` 返回 `Task<int>`，使用 `await dialogue.ShowTextTask(...)` 获取结果
- `DialogueManager.ShowGenericDialogue()` 提供便捷方法，自动创建、显示并销毁对话框
- `MenuDialogue`、`GenericDialogue` 都支持在同一实例中多次调用，允许重复使用
- `Game`、`Combat` 等业务流程围绕 `await` 语法构建，从而保持代码线性可读
- `DialogueManager` 与 `GameServer` 协作，使玩家输入与 MCP 自动化共用同一等待逻辑

