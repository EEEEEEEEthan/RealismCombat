# UI 系统

## 设计原则

- 所有 UI 组件均在构造函数中通过代码创建节点结构，不依赖场景文件
- 使用 `[Tool, GlobalClass]` 属性使组件可在编辑器中使用
- 这种方式提供更好的代码控制和类型安全

## 对话框管理 (DialogueManager)

`DialogueManager` 作为 Godot AutoLoad 单例提供统一的对话框管理：
- 位于 `Scripts/AutoLoad/` 目录
- 在 `project.godot` 中配置为自动加载单例，游戏启动时自动初始化
- 使用堆栈管理多个对话框，支持对话框的堆叠显示
- **统一管理输入**：在 `_Input()` 方法中接收所有输入事件，并分发给栈顶对话框处理
- 只有栈顶的对话框能响应玩家输入，其他对话框被遮挡
- 提供工厂方法创建对话框：`CreateGenericDialogue()` 和 `CreateMenuDialogue()`

### 对话框基类 (BaseDialogue)

- 所有对话框继承自 `BaseDialogue` 抽象类
- 提供 `Close()` 方法关闭对话框
- 在 `Dispose()` 时触发 `OnDisposing` 事件，由 `DialogueManager` 监听并从堆栈中移除
- 使用事件机制解耦对话框与管理器的依赖关系
- 子类重写 `HandleInput()` 方法处理输入，由 `DialogueManager` 调用

## 文字打印机 (Printer)

`Printer` 组件继承自 `RichTextLabel`，提供逐字打印效果：
- 支持可配置的打印间隔
- 提供 `Printing` 属性用于查询是否正在打印
- 打字音效通过 `AudioManager` 播放

## 通用对话框 (GenericDialogue)

`GenericDialogue` 组件提供通用的对话框功能：
- 继承自 `BaseDialogue`，通过 `DialogueManager.CreateGenericDialogue()` 创建
- 在构造函数中创建完整的节点层级结构
- 支持多段文本的追加显示（新文本追加到现有文本之后）
- 使用 `Printer` 组件实现打字机效果
- 显示向下箭头图标指示玩家可以继续（闪烁效果）
- 玩家按键后追加显示下一段文本
- 长按任意键可加速文本显示（将打印间隔设为0）

## 菜单对话框 (MenuDialogue)

`MenuDialogue` 组件提供交互式菜单功能：
- 继承自 `BaseDialogue`，通过 `DialogueManager.CreateMenuDialogue()` 创建
- 在构造函数中创建完整的节点层级结构（选项容器、描述文本、箭头指示器）
- 支持动态添加和清除选项（`AddOption`、`ClearOptions`）
- 使用上下方向键在选项间循环导航
- 使用 `Printer` 组件显示当前选项的描述文本
- 三角箭头指示器自动对齐到当前选中的选项
- 支持为每个选项绑定点击回调函数

## 异步等待机制

对话框和游戏节点支持通过 `await` 等待用户交互完成：

### GameNode

- 基础可等待节点类，继承自 `Node`
- 使用 `TaskCompletionSource` 实现异步等待
- 提供 `SetResult()`、`SetException()`、`SetCanceled()` 方法控制完成状态
- 提供 `Reset()` 方法重置状态以便重复使用

### MenuDialogue 异步等待

- 可通过 `await` 等待用户选择，返回选中的选项下标 (int)
- 用户按下 `ui_accept` 时自动完成等待并返回当前选中的下标
- 支持反复等待：每次 `await` 时会自动检测并重置已完成的状态
- 无需手动调用 `Reset()` 即可多次等待同一个菜单

### GenericDialogue 异步等待

- 可通过 `await` 等待用户看完所有文本，返回 bool
- 用户看完所有文本后按键时自动完成等待
- 提供 `SetResult()` 方法手动完成等待
- 提供 `Reset()` 方法重置状态以便重复使用

