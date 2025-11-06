# MCP服务器设计

## 目录

- [概述](#概述)
- [架构](#架构)
- [启动流程](#启动流程)
- [工具类](#工具类)

## 概述

游戏通过MCP(Model Context Protocol)服务器进行外部控制和自动化测试。

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
- 接收客户端命令并等待响应
- 通过静态事件 `OnCommandReceived` 将 `McpCommand` 发送到主线程处理
- 提供静态方法 `SendResponse()` 供命令处理者调用，发送响应
- 使用 `TaskCompletionSource` 等待命令处理完成
- 自动收集从命令接收到 `SendResponse()` 调用期间的所有日志并随响应返回

### 命令处理器 (Scripts/Nodes/)
- CommandHandlerNode：负责处理MCP命令的节点
- 由 ProgramRootNode 在检测到 `LaunchArgs.port` 时创建
- 订阅 GameServer 的静态事件（OnConnected、OnDisconnected、OnCommandReceived）
- 在主线程的 `_Process()` 中处理命令队列
- 处理逻辑：通过 `Log.Print()` 输出结果，然后调用静态方法 `GameServer.SendResponse()` 发送响应

### 通讯协议 (Scripts/)
- McpCommand：命令结构体，封装命令名称和参数
- 序列化格式：`command key1 value1 key2 value2`
- 支持无参数命令（仅包含命令名称）
- 提供 `TryGetArg()` 和 `GetArgOrDefault()` 方法访问参数

## 启动流程

1. MCP客户端分配空闲端口
2. 启动Godot进程并传递 `--port=xxx` 参数
3. 游戏启动TCP服务器监听该端口
4. MCP客户端连接到游戏
5. 双向通信建立完成

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

