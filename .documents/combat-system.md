# 战斗系统

## 战斗循环

- `Combat` 构造时会创建玩家输入与 AI 输入器，并调用 `StartLoop()` 进入主循环
- 开场创建 `GenericDialogue` 显示“战斗开始了!”，等待玩家或 MCP 确认后继续
- 每一帧循环流程：
  - 先执行 `CheckBattleOutcome()`，任一阵营全灭即结束战斗并完成等待器
  - 遍历仍存活的角色，调用其当前 `combatAction.UpdateTask()` 处理后摇逻辑
  - 重复调用 `TryGetActor()`，寻找行动点达到上限的角色并驱动决策流程
  - 所有行动完成后 `await Task.Delay(100)`，将 `Time` 增加 0.1，并按 `speed.value * 0.1` 回复行动点
- 循环未设置 tick 上限，只要双方仍有角色存活就会持续运行
- 异常会被捕获并记录日志，同时结束战斗等待器，避免主流程卡死

## 行动决策

- `CombatInput` 是输入基类，提供阵营、目标筛选等辅助方法
- `PlayerInput` 的决策步骤：
  - 弹出仅包含“攻击”的 `MenuDialogue`，为后续扩展留下入口
  - 生成敌方存活单位列表，通过 `MenuDialogue` 支持 ESC 返回
  - 选择目标后，再以同样方式从对方存活的身体部位中选择攻击目标
  - 返回 `Attack` 行动，外部在 `StartTask()` 时即时扣除前摇成本
- `AIInput` 采用完全随机策略：
  - 从敌方存活单位与对应可攻击部位中随机抽取目标
  - 若没有可用目标会抛出异常，战斗循环会捕获并记录
- 所有菜单交互均由 `DialogueManager` 托管，MCP 模式下可通过 `game_select_option` 指令驱动

## 行动前后摇

- `CombatAction` 抽象类封装统一的前后摇机制，构造函数中的两个浮点参数分别表示前摇与后摇成本
- `StartTask()` 会立即扣除前摇行动点，并执行 `OnStartTask()`，用于前摇动画或提示
- `UpdateTask()` 每帧检查行动点是否重新蓄满，若满足会扣除后摇成本并调用 `OnExecute()`
- `Attack` 的执行过程：
  - 创建 `GenericDialogue` 描述攻击动作，并等待打印完成
  - 计算 1~3 点伤害，先作用到被选中的身体部位，再同步角色总生命值
  - 如 `CombatNode` 中存在对应 `CharacterNode` 会触发 `Shake()` 动画，同时播放受击音效
  - 追加文本说明部位状态、角色是否倒下，并在对话框关闭后额外扣除 5 点行动点
- 外部可通过将 `character.combatAction` 置空中断后摇，例如战斗流程被强制终止

## 角色与属性

- `Character` 包含：
  - `PropertyInt hp`：角色总生命，实时汇总所有身体部位的生命值
  - `PropertyInt speed`：行动点回复速度
  - `PropertyDouble actionPoint`：当前与最大行动点
  - 六个 `BodyPart`，每个都实现 `ICombatTarget`
- `BodyPart` 维护独立生命值，用于位置化伤害表现；`GetName()` 提供中文显示名称
- `IsAlive` 基于身体部位存活状态判断，只有全部部位失去战斗能力才会倒下
- 所有属性支持二进制序列化，使用 `ReadScope()`、`WriteScope()` 确保数据长度正确
- `combatAction` 字段指向当前行动，用于战斗循环更新

## 战斗界面

- `CombatNode` 提供战斗 UI 框架：
  - `PlayerTeamContainer` 与 `EnemyTeamContainer` 分别展示我方与敌方
  - `Initialize()` 会清空旧节点、实例化新的 `CharacterNode` 并记录映射
  - `TryGetCharacterNode()` 支持按角色查找 UI 节点，攻击流程根据它触发动画
- `CharacterNode` 展示角色信息：
  - `NameLabel` 显示角色名称
  - `PropertyNode` 显示行动点与生命值，`Jump` 属性用于表现行动状态
  - 提供 `MoveTo()`、`Shake()` 等动画接口，可在后续加入更多战斗表现
- `PropertyNode` 内置 Shader 实现“跳动”效果，通过 `Jump` 开关控制，便于提示正在行动的角色

## 战斗入口

- 在 `Game` 菜单选择“开始战斗”时，项目会：
  - 从 `ResourceTable` 实例化 `CombatNode`
  - 创建 `Combat` 并传入默认的盟友与敌人编队
  - 等待战斗完成后释放 `CombatNode`，最后返回主菜单
- 任意异常都会被捕获并记录，确保 `Game` 循环能够恢复继续

## 日志与提示

- 战斗流程中的关键信息通过 `Log.Print` 记录，可在 MCP 模式下随响应返回
- 所有玩家提示使用 `GenericDialogue` 或 `MenuDialogue` 呈现，保证 UI 与 MCP 能共享同一逻辑
- 当指定 `LaunchArgs.port` 时，`GenericDialogue` 会自动跳过玩家输入，便于自动化测试

## 自动化与 MCP

- `Combat` 内部在需要等待外部输入时会调用 `GameServer.McpCheckpoint()`，让 MCP 客户端获知当前状态
- `CommandHandler` 将 `game_select_option` 翻译为 `MenuDialogue.SelectAndConfirm()`，实现远程操控
- 战斗日志依赖 `LogListener` 收集，在 MCP 响应中返回完整文本，方便分析回放
