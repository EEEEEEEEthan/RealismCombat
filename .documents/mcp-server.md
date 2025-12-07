# MCP服务器设计

## 目录

- [概述](#概述)
- [架构](#架构)
- [启动流程](#启动流程)
- [工具类](#工具类)

## 概述

游戏通过 MCP (Model Context Protocol) 服务器进行外部控制和自动化测试，核心目标包括：
- 允许外部代理驱动菜单选择
- 在每次选择前后收集完整日志
- 暴露调试命令，辅助排查现场状态

## 架构

### MCP客户端
MCP客户端是外部工具，通过MCP协议与游戏通信：
- 负责启动游戏进程
- 分配和管理端口
- 通过TCP连接与游戏通信
- 提供MCP工具接口供AI使用

### 启动参数管理 (Scripts/)
- LaunchArgs：静态类，解析并存储命令行参数
- 在静态构造函数中解析 `--port=xxx` 参数
- 提供 `port` 静态字段供其他组件访问

### 游戏端服务器 (Scripts/AutoLoad/)
- GameServer：作为 Godot AutoLoad 单例的 TCP 服务器
- 在 `_Ready()` 中检查 `LaunchArgs.port` 决定是否启动服务器
- 提供静态 API：`OnConnected`、`OnDisconnected`、`OnCommandReceived`、`SendResponse()`
- 接收客户端命令并等待响应，所有回调都在后台线程执行，主线程通过事件处理
- 通过静态事件 `OnCommandReceived` 将 `McpCommand` 发送到主线程处理
- 提供静态方法 `SendResponse()` 供命令处理者调用，发送响应
- 使用 `TaskCompletionSource` 等待命令处理完成，确保一次只能处理一条命令
- 自动收集从命令接收到 `SendResponse()` 调用期间的所有日志并随响应返回
- `LogListener` 订阅 `Log` 事件，`StopCollecting()` 后拼接所有日志文本

### 通讯协议 (Scripts/)

#### McpCommand 结构体

`McpCommand` 位于 `Scripts/McpCommand.cs`，是命令结构体，封装命令名称和参数。

##### 功能

- 封装命令名称和参数字典
- 支持序列化和反序列化
- 提供便捷的参数访问方法

##### 结构

```csharp
public readonly struct McpCommand
{
    public string Command { get; init; }
    public IReadOnlyDictionary<string, string> Args { get; init; }
}
```

##### 序列化格式

命令序列化格式为：`command key1 value1 key2 value2`

- 命令名称：第一个词，必需
- 参数：后续成对的键值对，可选
- 如果参数数量不是偶数，会输出错误日志并返回仅包含命令名称的命令

##### 构造方法

- `McpCommand(string command, IReadOnlyDictionary<string, string>? args = null)`：使用命令名称和参数创建命令
- `McpCommand.Deserialize(string data)`：从字符串反序列化命令

##### 序列化方法

- `Serialize()`：将命令序列化为字符串
- `ToString()`：返回序列化后的字符串

##### 参数访问

- `TryGetArg(string key, out string value)`：尝试获取参数值，返回是否成功
- `GetArgOrDefault(string key, string defaultValue = "")`：获取参数值，如果不存在返回默认值

##### 使用示例

```csharp
// 创建无参数命令
var cmd1 = new McpCommand("system_launch_program");

// 创建带参数命令
var cmd2 = new McpCommand("game_select_option", new Dictionary<string, string>
{
    { "id", "0" }
});

// 从字符串反序列化
var cmd3 = McpCommand.Deserialize("game_select_option id 0");

// 访问参数
if (cmd3.TryGetArg("id", out var id))
{
    var index = int.Parse(id);
}

// 或使用默认值
var id = cmd3.GetArgOrDefault("id", "0");
```

##### 反序列化细节

- 使用空格分割字符串
- 第一个词作为命令名称
- 后续成对的词作为参数键值对
- 如果参数数量不是偶数，输出错误日志并返回仅包含命令名称的命令

### 命令处理器 (Scripts/Nodes/)

#### CommandHandler 类

`CommandHandler` 位于 `Scripts/Nodes/CommandHandler.cs`，负责处理 MCP 命令的节点。

##### 功能

- 在 `_Process()` 中轮询命令队列，确保命令处理在主线程执行
- 订阅 `GameServer` 的静态事件（`OnConnected`、`OnDisconnected`、`OnCommandReceived`）
- 处理命令并发送响应
- 提供调试命令，便于排查问题

##### 生命周期

- 由 `ProgramRoot` 在检测到 `LaunchArgs.port` 时创建
- 在 `_Ready()` 中完成初始化并设置服务器回调
- 在 `_Process()` 中处理命令队列

##### 命令队列

- 使用 `ConcurrentQueue<McpCommand>` 存储待处理命令
- 在 `_Process()` 中轮询队列，一次处理一个命令
- 确保命令处理在主线程执行，避免线程安全问题

##### 支持的命令

###### system_launch_program

启动主循环，相当于玩家进入游戏。

```csharp
case "system_launch_program":
    programRoot.StartGameLoop();
    break;
```

###### debug_get_scene_tree

输出场景树的 JSON 结构，便于外部分析。

- 使用 `System.Text.Json` 序列化场景树
- 输出缩进的 JSON 格式
- 递归遍历所有子节点

```csharp
case "debug_get_scene_tree":
    var treeJson = GetSceneTreeJson();
    Log.Print(treeJson);
    GameServer.McpCheckpoint();
    break;
```

###### debug_get_node_details

打印指定节点的详细信息。

- 使用 `GD.VarToStr()` 获取节点状态
- 支持绝对路径和相对路径
- 如果节点不存在，返回错误信息

```csharp
case "debug_get_node_details":
    Log.Print(GetNodeDetails(cmd.Args["nodePath"]));
    GameServer.McpCheckpoint();
    break;
```

###### game_select_option

选择并确认当前对话框的选项。

- 从参数中获取选项索引（`id`）
- 若当前对话框是 `MenuDialogue`，调用 `SelectAndConfirm`
- 若当前对话框是 `GenericDialogue`，调用 `SelectAndConfirm`
- 其他对话框会打印错误并立即触发 `McpCheckpoint()`，防止客户端长时间等待

```csharp
case "game_select_option":
    var index = int.Parse(cmd.Args["id"]);
    var dialogue = DialogueManager.GetTopDialogue();
    switch (dialogue)
    {
        case MenuDialogue menuDialogue:
            menuDialogue.SelectAndConfirm(index);
            break;
        case GenericDialogue genericDialogue:
            genericDialogue.SelectAndConfirm(index);
            break;
        default:
            Log.PrintError("当前对话框不支持远程选项");
            GameServer.McpCheckpoint();
            break;
    }
    break;
```

##### 错误处理

- 所有命令处理都包裹在 `try-catch` 块中
- 捕获异常后，通过 `Log.PrintException()` 记录异常信息
- 调用 `GameServer.McpCheckpoint()` 发送响应，避免客户端一直等待
- 确保异常不会中断命令处理流程

##### 辅助方法

###### GetSceneTreeJson()

构建场景树的 JSON 表示。

```csharp
string GetSceneTreeJson()
{
    var root = GetTree().Root;
    var treeDict = BuildNodeTree(root);
    return JsonSerializer.Serialize(treeDict, new JsonSerializerOptions { WriteIndented = true });
}
```

###### BuildNodeTree(Node node)

递归构建节点树字典。

- 返回匿名对象，包含子节点名称和树结构
- 如果节点没有子节点，返回空对象

###### GetNodeDetails(string nodePath)

获取节点详细信息。

- 支持绝对路径（以 `/` 开头）和相对路径
- 相对路径会自动转换为 `/root/{nodePath}` 格式
- 如果节点不存在，返回错误信息
- 使用 `GD.VarToStr()` 获取节点状态

##### 使用示例

```csharp
// CommandHandler 由 ProgramRoot 自动创建
// 在 ProgramRoot._Ready() 中：
if (LaunchArgs.port.HasValue)
    AddChild(new CommandHandler(this));

// 命令处理流程：
// 1. MCP 客户端发送命令字符串
// 2. GameServer 反序列化为 McpCommand
// 3. 通过 OnCommandReceived 事件发送到 CommandHandler
// 4. CommandHandler 在 _Process() 中处理命令
// 5. 通过 Log.Print() 输出结果
// 6. 调用 GameServer.McpCheckpoint() 发送响应
```

## 指令一览

- `system_launch_program`：启动主循环，相当于玩家进入游戏
- `debug_get_scene_tree`：输出缩进的 JSON，展示当前场景树结构
- `debug_get_node_details nodePath=<path>`：打印节点的 `GD.VarToStr()` 结果
- `game_select_option id=<index>`：选择并确认当前 `MenuDialogue` 的选项
- 根据需要可以继续扩展其它命令，遵循字符串+键值对的协议格式

## 启动流程

1. MCP客户端分配空闲端口
2. 启动Godot进程并传递 `--port=xxx` 参数，`LaunchArgs` 会在静态构造函数解析
3. 游戏启动TCP服务器监听该端口，并输出初始化日志
4. MCP客户端连接到游戏，`GameServer` 触发 `OnConnected` 事件
5. `CommandHandler` 开始在 `_Process()` 中轮询命令队列
6. 客户端发送命令，游戏处理并通过 `SendResponse()` 返回日志

## 错误处理与恢复

- `GameServer` 在读取命令或写入响应时出现异常，会记录日志并关闭当前连接
- `CommandHandler` 捕获命令执行异常后，会打印堆栈并调用 `McpCheckpoint()`，确保客户端收到反馈
- 如果 MCP 客户端断开连接，`OnDisconnected` 会被触发，可在日志中看到提示
- 服务器关闭时会释放全部资源并取消后台任务，保证下次启动不会残留端口占用

## 工具类

### Log
统一的日志输出接口，封装GD.Print系列方法：
- 自动添加时间戳 `[HH:mm:ss]`
- 提供三个事件：OnLog、OnLogError、OnLogWarning
- 支持通过事件订阅日志输出

### LogListener
日志收集器，用于收集一段时间内的日志：
- 通过订阅Log事件收集日志
- 线程安全的日志收集和读取
- 自动添加日志级别标记（ERROR、WARN）

### Settings
配置管理，从`.local.settings`文件读取配置（格式：`key = value`）。

