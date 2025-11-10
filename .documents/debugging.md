# 调试指南

## MCP 调试工具

项目提供 MCP (Model Context Protocol) 服务器用于自动化测试与远程操控。详见 [MCP 服务器文档](mcp-server.md)。

### 工作流程概览

1. MCP 客户端启动游戏并等待 `GameServer` 建立 TCP 连接
2. 当游戏需要输入时调用 `GameServer.McpCheckpoint()`，客户端即可拉取日志
3. 客户端通过指令（例如 `game_select_option`）驱动下一步选择
4. 游戏继续运行至下一次检查点，并返回期间累积的日志文本

### 常用命令

- `system_launch_program`：确保主循环已启动
- `debug_get_scene_tree`：输出当前场景树 JSON，排查节点层级
- `debug_get_node_details nodePath=/root/ProgramRoot`：检查节点序列化信息
- `game_select_option id=0`：模拟菜单选择，索引从 0 开始

### 日志收集

- `LogListener` 会在命令执行期间订阅日志事件，响应时自动附带全部输出
- 调试时可适度插入 `Log.Print()`，但应保持内容简洁，方便定位关键路径
- 异常应通过 `Log.PrintException()` 记录，便于在 MCP 返回中查看堆栈

## 本地调试建议

- 使用 Godot 编辑器或命令行启动游戏时，可观察控制台中的时间戳日志
- 通过 `DialogueManager.GetDialogueCount()` 检查是否存在未释放的对话框
- 战斗 UI 显示异常时，可定位 `CombatNode`、`CharacterNode` 对应的场景与脚本
- 如需模拟音效播放情况，调用 `AudioManager.IsSfxPlaying` 监控当前状态

## 断点与日志配合

- 在 VS、Rider 等 IDE 中设置断点时，可使用 `Debugger.Break()` 辅助定位
- 对需要重复验证的逻辑，应同时保留关键日志，便于自动化测试重放
- 关注 `GameServer`、`CommandHandler` 的日志输出，可以快速判断 MCP 指令是否正确抵达主线程

## 常见问题定位

- **未出现菜单**：确认当前对话框是否被正确关闭，或是否仍有 MCP 命令在排队
- **战斗循环卡死**：查看日志中是否存在未捕获异常，检查 `combatAction` 是否被正确清空
- **资源加载失败**：确认资源是否已在 `ResourceTable` 注册，路径是否与实际文件一致
- **音频无声**：检查 `AudioManager` 是否初始化成功，或全局音量是否被设为过低值
