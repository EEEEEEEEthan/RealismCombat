# RealismCombat 项目文档

## 目录

- [开发规范](#开发规范)
  - [节点命名规范](#节点命名规范)
- [核心系统](#核心系统)
  - [资源管理](#资源管理)
  - [音频管理](#音频管理)
  - [UI 系统](#ui-系统)
  - [扩展方法](#扩展方法)
- [MCP 服务器](#mcp-服务器)

## 开发规范

### 节点命名规范

**所有在代码中动态创建的节点都必须赋予有意义的名字：**

- 使用 `Name` 属性为节点设置清晰、描述性的名字
- 对于自定义节点类，在 `_EnterTree()` 方法中设置 `Name`
- 对于内置节点类（如 `AudioStreamPlayer`），可在对象初始化器中设置或在父节点中设置
- 避免使用默认生成的名字（如 `@ClassName@ID`）

**原因：**
- 便于通过场景树调试和定位问题
- 提高代码可维护性和可读性
- 方便使用节点路径获取节点

**示例：**
```csharp
// 自定义节点类
public override void _EnterTree()
{
    base._EnterTree();
    Name = "Dialogue";
}

// 内置节点类
var player = new AudioStreamPlayer()
{
    Name = "BgmPlayer",
    Bus = "Master",
};
```

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

`AudioManager` 作为 Godot AutoLoad 单例提供统一的音频管理：
- 位于 `Scripts/AutoLoad/` 目录
- 在 `project.godot` 中配置为自动加载单例，游戏启动时自动初始化
- 使用两个 `AudioStreamPlayer` 分别管理背景音乐（BGM）和音效（SFX）
- 提供静态方法访问，无需获取实例即可使用
- 支持播放、停止、音量控制等功能

**使用规则：**
- 所有音频资源必须在 `ResourceTable` 中定义
- 直接调用静态方法 `AudioManager.PlayBgm()` 和 `AudioManager.PlaySfx()` 播放音频
- 避免在各个组件中独立创建 `AudioStreamPlayer`

### UI 系统

**设计原则：**
- 所有 UI 组件均在构造函数中通过代码创建节点结构，不依赖场景文件
- 使用 `[Tool, GlobalClass]` 属性使组件可在编辑器中使用
- 这种方式提供更好的代码控制和类型安全

#### 对话框管理 (DialogueManager)

`DialogueManager` 作为 Godot AutoLoad 单例提供统一的对话框管理：
- 位于 `Scripts/AutoLoad/` 目录
- 在 `project.godot` 中配置为自动加载单例，游戏启动时自动初始化
- 使用堆栈管理多个对话框，支持对话框的堆叠显示
- **统一管理输入**：在 `_Input()` 方法中接收所有输入事件，并分发给栈顶对话框处理
- 只有栈顶的对话框能响应玩家输入，其他对话框被遮挡
- 提供工厂方法创建对话框：`CreateGenericDialogue()` 和 `CreateMenuDialogue()`

**对话框基类 (BaseDialogue)：**
- 所有对话框继承自 `BaseDialogue` 抽象类
- 提供 `Close()` 方法关闭对话框
- 在 `Dispose()` 时触发 `OnDisposing` 事件，由 `DialogueManager` 监听并从堆栈中移除
- 使用事件机制解耦对话框与管理器的依赖关系
- 子类重写 `HandleInput()` 方法处理输入，由 `DialogueManager` 调用

#### 文字打印机 (Printer)

`Printer` 组件继承自 `RichTextLabel`，提供逐字打印效果：
- 支持可配置的打印间隔
- 提供 `Printing` 属性用于查询是否正在打印
- 打字音效通过 `AudioManager` 播放

#### 通用对话框 (GenericDialogue)

`GenericDialogue` 组件提供通用的对话框功能：
- 继承自 `BaseDialogue`，通过 `DialogueManager.CreateGenericDialogue()` 创建
- 在构造函数中创建完整的节点层级结构
- 支持多段文本的追加显示（新文本追加到现有文本之后）
- 使用 `Printer` 组件实现打字机效果
- 显示向下箭头图标指示玩家可以继续（闪烁效果）
- 玩家按键后追加显示下一段文本
- 长按任意键可加速文本显示（将打印间隔设为0）

#### 菜单对话框 (MenuDialogue)

`MenuDialogue` 组件提供交互式菜单功能：
- 继承自 `BaseDialogue`，通过 `DialogueManager.CreateMenuDialogue()` 创建
- 在构造函数中创建完整的节点层级结构（选项容器、描述文本、箭头指示器）
- 支持动态添加和清除选项（`AddOption`、`ClearOptions`）
- 使用上下方向键在选项间循环导航
- 使用 `Printer` 组件显示当前选项的描述文本
- 三角箭头指示器自动对齐到当前选中的选项
- 支持为每个选项绑定点击回调函数

### 扩展方法

项目提供了一些实用的扩展方法，定义在 `Scripts/Extensions/` 目录下，使用 `partial class` 模式组织：

- `IDisposableExtensions.TryDispose()`：安全地释放资源，捕获并记录异常
  - 用于需要释放资源但不希望异常中断程序流程的场景
  - 异常会通过 `Log.PrintException()` 记录

- `ActionExtensions.TryInvoke<T>()`：安全地调用事件或委托，捕获并记录异常
  - 用于需要触发事件但不希望订阅者的异常中断程序流程的场景
  - 异常会通过 `Log.PrintException()` 记录

## MCP 服务器

详见 [mcp-server.md](mcp-server.md)

