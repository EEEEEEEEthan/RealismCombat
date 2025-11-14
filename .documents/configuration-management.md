# 配置管理

## 概述

配置管理系统负责处理游戏启动时的配置加载和命令行参数解析，包括本地配置文件读取和启动参数管理。

## 核心类

### Settings 类

`Settings` 是静态配置管理类，位于 `Scripts/Settings.cs`：

#### 功能

- 从 `.local.settings` 文件读取配置项
- 支持 `key = value` 格式的配置
- 使用正则表达式解析配置行
- 在静态构造函数中自动加载配置
- 配置加载时输出日志，便于确认配置是否生效

#### 配置格式

配置文件位于项目根目录的 `.local.settings` 文件，格式为：

```
key1 = value1
key2 = value2
```

每行一个配置项，格式为 `key = value`，等号前后可以有空格。

#### API

- `Get(string key)`：获取配置值，如果不存在返回 `null`
- `Get(string key, string defaultValue)`：获取配置值，如果不存在返回默认值

#### 使用示例

```csharp
// 读取配置
var godotPath = Settings.Get("godot");
if (godotPath != null)
{
    Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
}

// 读取配置，带默认值
var debugMode = Settings.Get("debug", "false");
```

#### 实现细节

- 静态构造函数中会获取项目根目录路径
- 使用 `ProjectSettings.GlobalizePath("res://")` 获取项目根目录
- 配置文件路径为 `{项目根目录}/.local.settings`
- 如果配置文件不存在，会输出错误日志但不抛出异常
- 使用生成的正则表达式 `ConfigRegex()` 匹配配置行
- 成功解析的配置会输出到日志，便于调试

### LaunchArgs 类

`LaunchArgs` 是启动参数解析器，位于 `Scripts/LaunchArgs.cs`：

#### 功能

- 解析 Godot 命令行参数
- 提取并存储关键启动参数（如端口号）
- 在静态构造函数中自动解析参数
- 输出解析日志，便于确认参数是否生效

#### 支持的参数

- `--port=xxx`：指定 MCP 服务器监听端口

#### API

- `port`：静态只读字段，类型为 `int?`，表示解析到的端口号

#### 使用示例

```csharp
if (LaunchArgs.port.HasValue)
{
    Log.Print($"[GameServer] 初始化服务器，端口: {LaunchArgs.port.Value}");
    // 启动 MCP 服务器
}
else
{
    Log.Print("[GameServer] 未指定端口，服务器不启动");
}
```

#### 实现细节

- 使用 `OS.GetCmdlineUserArgs()` 获取用户命令行参数
- 解析 `--port=` 前缀的参数，提取端口号
- 如果端口号无效，会输出错误日志
- 如果没有指定端口，会输出提示日志
- 所有参数解析都会输出到日志，便于调试

## 配置与启动流程

1. **程序启动**：
   - `LaunchArgs` 静态构造函数执行，解析命令行参数
   - `Settings` 静态构造函数执行，加载 `.local.settings` 配置文件

2. **参数使用**：
   - `ProgramRoot._Ready()` 中检查 `LaunchArgs.port` 决定是否启动 MCP 模式
   - `GameServer._Ready()` 中使用 `LaunchArgs.port` 初始化服务器
   - 其他地方通过 `Settings.Get()` 读取配置项

3. **配置优先级**：
   - 命令行参数优先级高于配置文件
   - 配置文件适用于本地开发环境设置（如 Godot 路径、调试开关等）

## 注意事项

- `.local.settings` 文件通常不应提交到版本控制系统，应添加到 `.gitignore`
- 配置文件中不支持多行值或特殊转义
- 如果配置行格式不正确（不符合 `key = value`），会被忽略
- 命令行参数解析是静态的，程序启动后不会重新解析
- `Settings` 和 `LaunchArgs` 都是静态类，在首次访问时自动初始化

## 配置示例

`.local.settings` 文件示例：

```
godot = C:\Godot\Godot_v4.3.3-stable_mono_win64.exe
debug = true
mcp_port = 12345
```

启动游戏时传入参数：

```
--port=54321
```

在这种情况下，MCP 服务器会使用命令行参数指定的端口 `54321`，而不是配置文件中的 `12345`。

