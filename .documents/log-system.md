# 日志系统

## 概述

日志系统提供了统一的日志输出和收集机制，支持不同级别的日志（普通、警告、错误），并可以监听和收集日志内容。

## 核心类

### Log 类

`Log` 是静态工具类，位于 `Scripts/Log.cs`，提供统一的日志输出接口：

#### 事件

- `OnLog`：普通日志事件，在 `Print()` 时触发
- `OnLogWarning`：警告日志事件，在 `PrintWarning()` 时触发
- `OnLogError`：错误日志事件，在 `PrintError()` 或 `PrintException()` 时触发

#### 方法

##### Print()

输出普通日志信息：
- 参数：`params object[] args`
- 功能：将所有参数拼接成字符串，添加时间戳后输出到 Godot 控制台
- 触发事件：`OnLog`

##### PrintError()

输出错误日志信息：
- 参数：`params object[] args`
- 功能：将所有参数拼接成字符串，添加时间戳和调用栈后输出到 Godot 错误控制台
- 触发事件：`OnLogError`
- 调用栈：使用 `StackTrace(1, true)` 获取调用栈信息

##### PrintWarning()

输出警告日志信息：
- 参数：`params object[] args`
- 功能：将所有参数拼接成字符串，添加时间戳后输出到 Godot 警告控制台
- 触发事件：`OnLogWarning`

##### PrintException()

输出异常信息：
- 参数：`Exception ex`
- 功能：格式化异常信息（类型、消息、堆栈），添加时间戳后输出到 Godot 错误控制台
- 触发事件：`OnLogError`

#### 时间戳

所有日志输出都包含时间戳：
- 格式：`[HH:mm:ss]`
- 例如：`[14:30:25]`

### LogListener 类

`LogListener` 是可释放类，位于 `Scripts/Log.cs`，用于收集日志内容：

#### 功能

- 订阅 `Log` 的所有事件（`OnLog`、`OnLogWarning`、`OnLogError`）
- 在收集模式下，将所有日志消息保存到内部列表
- 提供方法开始和停止收集，返回收集到的日志文本

#### 方法

##### StartCollecting()

开始收集日志：
- 清空之前的日志列表
- 设置收集标志为 `true`
- 之后的所有日志都会被收集

##### StopCollecting()

停止收集日志：
- 设置收集标志为 `false`
- 返回收集到的所有日志，用换行符连接

##### Clear()

清空收集的日志列表。

##### Dispose()

释放资源：
- 取消订阅所有日志事件
- 防止内存泄漏

#### 线程安全

- 使用 `lock` 确保多线程环境下的安全性
- 所有操作都在锁内进行，保证数据一致性

#### 日志格式

收集的日志包含原始消息，错误日志会添加 `[ERROR]` 前缀，警告日志会添加 `[WARN]` 前缀。

## 使用场景

### 常规日志输出

```csharp
// 普通日志
Log.Print("玩家选择了选项 1");
Log.Print("角色", character.name, "的生命值：", hitPoint.value);

// 警告日志
Log.PrintWarning("配置项不存在：", key);

// 错误日志
Log.PrintError("无法加载资源：", path);
Log.PrintException(exception);
```

### 日志收集（MCP 模式）

在 MCP 模式下，`GameServer` 使用 `LogListener` 收集日志：

```csharp
var logListener = new LogListener();
logListener.StartCollecting();

// 执行命令...

var logs = logListener.StopCollecting();
// 将日志返回给 MCP 客户端

logListener.Dispose();
```

### 日志监听

可以订阅日志事件进行自定义处理：

```csharp
Log.OnLog += (message) =>
{
    // 自定义处理逻辑
    WriteToFile(message);
};

Log.OnLogError += (message) =>
{
    // 发送错误报告
    SendErrorReport(message);
};
```

## 设计原则

### 统一日志接口

- 所有日志输出都通过 `Log` 类，避免直接使用 `GD.Print`
- 保证日志格式一致，包含时间戳
- 便于后续扩展（如日志文件、远程日志等）

### 事件机制

- 通过事件机制解耦日志输出和日志处理
- 允许多个监听者同时处理日志
- 支持日志收集、文件记录、远程上报等多种用途

### 线程安全

- `LogListener` 使用锁确保线程安全
- 允许在后台线程收集日志，而日志输出可能在主线程

### 错误信息完整性

- 错误日志包含调用栈信息，便于调试
- 异常日志包含完整的异常信息（类型、消息、堆栈）

## 在 MCP 系统中的应用

### GameServer 日志收集

`GameServer` 在处理 MCP 命令时使用 `LogListener`：

1. 创建 `LogListener` 实例
2. 调用 `StartCollecting()` 开始收集
3. 处理命令（期间所有日志都被收集）
4. 调用 `StopCollecting()` 获取收集的日志
5. 将日志随响应返回给 MCP 客户端
6. 释放 `LogListener`

这样 MCP 客户端可以获取完整的命令执行日志，便于调试和分析。

## 扩展指南

### 添加日志级别

1. 在 `Log` 类中添加新的日志方法（如 `PrintDebug()`）
2. 添加对应的事件（如 `OnLogDebug`）
3. 在 `LogListener` 中订阅新事件

### 日志文件输出

可以添加日志文件输出功能：

```csharp
Log.OnLog += (message) =>
{
    File.AppendAllText("game.log", $"{DateTime.Now} {message}\n");
};
```

### 日志过滤

可以添加日志过滤功能：

```csharp
Log.OnLog += (message) =>
{
    if (message.Contains("DEBUG"))
    {
        // 只在调试模式下输出
    }
};
```

### 远程日志上报

可以添加远程日志上报功能：

```csharp
Log.OnLogError += async (message) =>
{
    await HttpClient.PostAsync("https://api.example.com/logs", ...);
};
```

