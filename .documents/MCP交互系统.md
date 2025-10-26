# MCP交互系统

## 概述
MCP(Model Context Protocol)交互系统允许外部工具通过TCP连接控制游戏，实现自动化测试和AI交互。

## 核心架构

### 1. McpHandler - TCP服务器
位于 `Scripts/McpHandler.cs`

**职责：**
- 监听本地TCP端口，接受单个客户端连接
- 接收文本格式的指令，转发给游戏状态机
- 收集指令执行期间的日志输出
- 在检查点将日志打包回复给客户端

**关键特性：**
- **单客户端模式**：同时只允许一个客户端连接，其他连接会被拒绝
- **单指令处理**：同时只处理一条指令，重复请求会收到"正忙"响应
- **日志收集**：通过`CommandLifeCycle`订阅`Log`事件，自动收集指令执行期间的所有日志
- **异步通信**：使用独立的异步任务处理网络I/O，不阻塞游戏主线程

**工作流程：**
1. 客户端发送指令字符串（如 `game_start_combat`）
2. `McpHandler`接收后设置`pendingCommand`
3. 游戏主循环`Update()`中检测到`pendingCommand`，创建`CommandLifeCycle`并开始收集日志
4. 调用状态机的`ExecuteCommand()`执行指令
5. 指令执行完毕后调用`McpCheckPoint()`
6. 将收集的所有日志打包回复给客户端

### 2. Command - 指令抽象
位于 `Scripts/Commands/Command.cs`

**职责：**
- 定义指令的基本结构
- 解析指令参数（格式：`command_name key1 value1 key2 value2`）
- 提供统一的执行接口

**已实现的指令：**
- `system_shutdown` - 关闭游戏 (`Scripts/Commands/SystemCommands/ShutdownCommand.cs`)
- `program_start_new_game` - 开始新游戏 (`Scripts/Commands/ProgramCommands/StartNewGameCommand.cs`)
- `game_start_combat` - 开始战斗 (`Scripts/Commands/GameCommands/StartCombatCommand.cs`)
- `game_quit_to_menu` - 退出到主菜单 (`Scripts/Commands/GameCommands/QuitGameCommand.cs`)
- `debug_show_node_tree` - 显示节点树结构 (`Scripts/Commands/DebugCommands/DebugShowNodeTreeCommand.cs`)

### 3. 状态机集成
位于 `Scripts/StateMachine/State.cs` 和 `Scripts/Nodes/ProgramRoot.cs`

**职责：**
- 每个状态定义自己支持的指令集
- 根据指令名称创建对应的Command对象并执行
- 状态切换时自动触发检查点，将日志回复给MCP客户端
- 所有状态默认支持 `system_shutdown` 和 `debug_show_node_tree` 指令

**状态与指令映射：**
- `MenuState`（主菜单状态）：支持 `program_start_new_game`
- `PrepareState`（准备状态）：支持 `game_start_combat`
- `GameState`（游戏状态）：支持子状态的所有指令 + `game_quit_to_menu`
- `CombatState`（战斗状态）：无额外指令

### 4. ProgramRoot集成
位于 `Scripts/Nodes/ProgramRoot.cs`

**启动流程：**
1. 从命令行参数读取 `--port=端口号`
2. 创建`McpHandler`实例并监听指定端口
3. 订阅客户端连接/断开事件
4. 在`_Process()`中每帧调用`mcpHandler.Update()`处理待执行指令

**生命周期管理：**
- 当客户端断开连接时，如果曾经有客户端连接过，则自动退出游戏
- 这确保了测试工具可以控制游戏的完整生命周期

## 关键概念

### McpCheckPoint（检查点）
检查点是MCP系统的核心同步机制：
- 指令执行完成后调用检查点
- 状态切换时自动调用检查点
- 检查点触发时，将收集的日志打包回复给客户端，结束本次指令处理

### CommandLifeCycle（指令生命周期）
用于追踪单次指令执行的完整过程：
- 创建时订阅`Log.OnLog`和`Log.OnError`事件
- 自动收集所有日志消息
- 销毁时取消订阅，避免内存泄漏
- 在检查点时将收集的消息合并返回

### 线程安全
- 使用`lock(sync)`保护客户端连接状态
- 使用`lock(writeSync)`保护网络写入操作
- `pendingCommand`通过锁机制在主线程和网络线程间安全传递

## 设计优势

1. **非阻塞**：网络I/O在独立线程，不影响游戏帧率
2. **日志自动收集**：无需手动管理日志，通过事件系统自动完成
3. **状态安全**：只执行当前状态允许的指令，防止非法操作
4. **简单协议**：文本格式，易于调试和扩展
5. **生命周期管理**：客户端断开自动退出，适合自动化测试

## 扩展指南

添加新指令的步骤：
1. 在`Scripts/Commands/`创建新的Command子类
2. 定义指令名称常量（如 `public const string name = "xxx"`）
3. 实现`Execute()`方法
4. 在对应状态类的`ExecuteCommand()`和`GetAvailableCommands()`中注册指令
5. 执行完成后调用`gameRoot.mcpHandler?.McpCheckPoint()`确保响应返回

