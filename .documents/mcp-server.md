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

### 命令处理器 (Scripts/Nodes/)
- CommandHandlerNode：负责处理MCP命令的节点
- 由 ProgramRootNode 在检测到 `LaunchArgs.port` 时创建
- 订阅 GameServer 的静态事件（OnConnected、OnDisconnected、OnCommandReceived）
- 在主线程的 `_Process()` 中处理命令队列
- 处理逻辑：通过 `Log.Print()` 输出结果，然后调用静态方法 `GameServer.SendResponse()` 发送响应
- 提供 `SelectAndConfirm()`，将 `game_select_option` 指令映射到当前菜单
- 使用 `System.Text.Json` 序列化场景树，便于外部分析
- 任意异常都会被捕获并记录，同时调用 `McpCheckpoint()`，避免客户端一直等待

### 通讯协议 (Scripts/)
- McpCommand：命令结构体，封装命令名称和参数
- 序列化格式：`command key1 value1 key2 value2`
- 支持无参数命令（仅包含命令名称）
- 提供 `TryGetArg()` 和 `GetArgOrDefault()` 方法访问参数

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

