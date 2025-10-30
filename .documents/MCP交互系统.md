# MCP交互系统

## 概述
MCP(Model Context Protocol)交互系统允许外部工具通过TCP连接控制游戏，实现自动化测试和AI交互。系统采用简单的命令-响应模式，通过正则表达式匹配命令。

## 核心架构

### McpHandler - TCP服务器
位于 `Scripts/McpHandler.cs`

**职责：**
- 监听本地TCP端口，接受单个客户端连接
- 接收文本格式的命令，转发给程序根节点
- 收集命令执行期间的日志输出
- 在响应点将日志打包回复给客户端

**关键特性：**
- **单客户端模式**：同时只允许一个客户端连接，其他连接会被拒绝
- **单命令处理**：同时只处理一条命令，重复请求会收到"正忙"响应
- **退出命令优先级**：`game_quit` 命令即使系统正忙也会立即处理，中断当前命令
- **日志收集**：通过 `CommandLifeCycle` 订阅 `Log` 事件，自动收集命令执行期间的所有日志
- **异步通信**：使用独立的异步任务处理网络I/O，不阻塞游戏主线程

**工作流程：**
1. 客户端发送命令字符串（如 `game_select_option 0`）
2. `McpHandler` 接收后设置 `pendingCommand`
3. 游戏主循环 `Update()` 中检测到 `pendingCommand`，创建 `CommandLifeCycle` 并开始收集日志
4. 调用 `ProgramRootNode.OnMcpRequest(command)` 执行命令
5. 命令执行完毕后调用 `McpRespond()`
6. 将收集的所有日志打包回复给客户端

### ProgramRootNode - 命令处理器
位于 `Scripts/Nodes/ProgramRootNode.cs`

**MCP集成：**
- 从命令行参数读取 `--port=端口号`
- 创建 `McpHandler` 实例并监听指定端口
- 订阅客户端连接/断开事件
- 在 `_Process()` 中每帧调用 `mcpHandler.Update()` 处理待执行命令

**命令处理：**
使用正则表达式匹配命令，支持以下命令：
- `system_launch_program` - 刷新当前对话框
- `game_select_option (\d+)` - 选择对话框选项
- `show_node_tree` - 显示节点树
- `game_quit` - 退出游戏

**生命周期管理：**
- 客户端首次连接时设置 `HadClientConnected = true`
- 客户端断开时，如果曾经连接过，则自动退出游戏
- 确保测试工具可以控制游戏的完整生命周期

## 关键概念

### McpRespond（响应点）
响应点是MCP系统的核心同步机制：
- 命令执行完成后调用 `McpRespond()`
- 对话框激活/确认时自动调用响应点
- 响应点触发时，将收集的日志打包回复给客户端，结束本次命令处理

### CommandLifeCycle（命令生命周期）
用于追踪单次命令执行的完整过程：
- 创建时订阅 `Log.OnLog` 和 `Log.OnError` 事件
- 自动收集所有日志消息到 `messages` 列表
- 销毁时取消订阅，避免内存泄漏
- 在响应点时将收集的消息合并返回

### 线程安全
- 使用 `lock(sync)` 保护客户端连接状态
- 使用 `lock(writeSync)` 保护网络写入操作
- `pendingCommand` 通过锁机制在主线程和网络线程间安全传递

## MCP服务端工具

### 工具定义
位于 `.McpServer/Tools.cs`

**SystemTools：**
- `system_launch_program` - 启动游戏程序
  - 自动编译项目
  - 创建 `GameClient` 实例管理进程和连接
  - 维护单例客户端，防止重复启动

**GameTools：**
- `game_select_option(int option_index)` - 选择对话框选项
  - 发送 `game_select_option <index>` 命令到游戏
- `game_quit()` - 退出游戏
  - 发送 `game_quit` 命令到游戏

**DebugTools：**
- `show_node_tree()` - 显示节点树
  - 发送 `show_node_tree` 命令到游戏

### GameClient - 游戏客户端
位于 `.McpServer/GameClient.cs`

**职责：**
- 编译并启动游戏进程
- 建立TCP连接
- 发送命令并等待响应
- 管理日志文件
- 监听进程退出和连接断开

## 命令格式

### 基本格式
命令使用简单的文本格式：
```
command_name [arguments]
```

### 示例
```
system_launch_program
game_select_option 0
game_select_option 1
show_node_tree
game_quit
```

## 设计优势

1. **简单协议**：纯文本命令，易于调试和扩展
2. **非阻塞**：网络I/O在独立线程，不影响游戏帧率
3. **日志自动收集**：无需手动管理日志，通过事件系统自动完成
4. **正则匹配**：灵活的命令解析，支持带参数的命令
5. **生命周期管理**：客户端断开自动退出，适合自动化测试

## 扩展指南

添加新命令的步骤：
1. 在 `ProgramRootNode.OnMcpRequest()` 中添加正则匹配分支
2. 实现命令逻辑
3. 完成后调用 `McpRespond()` 确保响应返回
4. 在 `.McpServer/Tools.cs` 中添加对应的MCP工具方法
5. 更新测试文档

