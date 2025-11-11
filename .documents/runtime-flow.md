# 运行时流程

## 启动入口

- `ProgramRoot` 作为场景树入口，在 `_Ready()` 中完成基础初始化并打印启动日志
- 通过 `Settings.Get()` 读取 `.local.settings`，可用于本地化 Godot 路径或调试开关
- 若 `LaunchArgs.port` 存在，则说明处于 MCP 驱动模式，会挂载 `CommandHandler` 处理远程指令
- 无 MCP 端口时直接调用 `StartGameLoop()`，进入本地交互流程

## 主菜单循环

### `ProgramRoot.StartGameLoop()`

- 使用永真循环维护应用生命周期，保证异常不会中断主流程
- 每轮调用内部的 `Routine()`，根据返回结果决定是否退出
- 循环尾部等待一帧后调用 `SceneTree.Quit()`，确保 Godot 正常收尾

### `Routine()` 菜单

- 创建主菜单对话框，提供“开始游戏”“读取游戏”“退出游戏”三项
- 菜单选项由 `MenuDialogue` 与 `DialogueManager` 驱动，玩家输入或 MCP 指令都会被统一处理
- 选择不同项会触发以下行为：
  - `开始游戏`：实例化 `GameNode` 场景，创建新的 `Game` 对象
  - `读取游戏`：检测 `save.dat` 是否存在，存在则读取存档并创建 `Game` 对象
  - `退出游戏`：通知 MCP 达到检查点，延迟一帧后优雅退出

### `Game` 生命周期

- `Game` 接受 `saveFilePath` 与 `gameNode`（通常为 `GameNode.tscn`）作为参数
- 新游戏与读取存档共用同一循环逻辑，通过 `Snapshot` 结构读写版本信息
- `GetAwaiter()` 暴露 `Task` 等待接口，可使用 `await game;` 监控流程结束
- 游戏循环中展示二级菜单：
  - `开始战斗`：加载 `CombatNode.tscn`，初始化盟友与敌人，并等待战斗结束
  - 新建游戏时默认生成的 `Hero` 会在右臂装备 `LongSword`，便于立即测试战斗动作
  - `查看状态`：暂未实现，当前显示提示文本
  - `退出游戏`：调用内部 `Quit()` 完成任务并返回主菜单

## 场景与 UI 结构

### `GameNode`

- 作为 `Game` 的宿主节点挂载到场景树
- 主要用于承载后续的战斗界面或其它子系统节点

### `CombatNode`

- 提供战斗界面的 UI 框架，包含玩家与敌人的角色栏容器
- `Initialize()` 清空旧节点后，为每个角色实例化 `CharacterNode` 并记录映射
- `TryGetCharacterNode()` 支持通过角色引用查找对应 UI，用于战斗动画或反馈

### `CharacterNode`

- 展示角色名称、生命值与行动值
- `MoveTo()`、`Shake()` 通过 `Tween` 实现平滑动画，用于表现技能或受击
- 在 `_Process()` 中同步角色状态，当角色行动中会点亮行动值的跳动特效

### `PropertyNode`

- 用于显示属性条，支持标题、当前值与最大值
- `Jump` 属性切换特殊 Shader 材质，提供闪烁提示（例如正在行动）
- Shader 源码内嵌于脚本，运行时动态创建共享材质实例

## 核心支撑组件

### `LaunchArgs`

- 解析 Godot 用户命令行参数，关注 `--port=XXXX`
- 若成功解析端口，将项目切换到 MCP 自动化模式，并打印详细日志

### `Settings`

- 在项目启动时读取 `.local.settings` 文件
- 支持 `key = value` 形式的配置，使用正则解析
- 所有键值会输出到日志，便于确认配置是否生效

### `Log` 与 `LogListener`

- `Log.Print()`、`PrintError()`、`PrintWarning()` 统一输出格式，并透传到事件
- `PrintException()` 捕获异常类型、消息与堆栈
- `LogListener` 订阅日志事件，在 MCP 请求处理期间收集日志并返回给客户端

### `GameServer`

- 当指定端口存在时作为 TCP 服务器运行
- 接收 MCP 指令，排他性维护单个客户端连接
- 每条指令配合 `LogListener` 收集执行过程
- 调用 `GameServer.McpCheckpoint()` 会写回日志，并唤醒等待中的响应任务

### `CommandHandler`

- 在 `_Process()` 中轮询命令队列，保证命令处理始终发生在主线程
- 支持以下指令：
  - `system_launch_program`：触发 `ProgramRoot.StartGameLoop()`
  - `debug_get_scene_tree`：返回格式化的场景树 JSON
  - `debug_get_node_details`：打印指定节点状态
  - `game_select_option`：模拟菜单确认，驱动 `MenuDialogue`
- 指令处理异常会记录日志并强制发送 MCP 检查点，避免客户端悬挂

## 异步等待模式

- `Game`、`Combat`、`MenuDialogue`、`GenericDialogue` 均实现自定义等待器
- 利用 `TaskCompletionSource` 管理生命周期，既可被 `await`，也可被 MCP 指令驱动
- `GameNode` 与 `CombatNode` 通过 `await` 等待确保节点释放时机，避免资源泄漏


