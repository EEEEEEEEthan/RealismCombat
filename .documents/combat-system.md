# 战斗系统

## 战斗循环

- `Combat` 构造时会初始化战斗界面节点，创建玩家与 AI 输入器，并立即启动异步的 `StartLoop()` 主循环
- 开场生成 `GenericDialogue` 显示“战斗开始了!”，等待玩家或 MCP 确认后才进入正式循环
- 主循环每帧执行：
  - 调用 `CheckBattleOutcome()` 判断任一阵营是否失去作战能力，若成立则结束战斗并解除等待
  - 遍历仍存活的角色，若存在 `combatAction` 则执行 `UpdateTask()` 推进后摇；当返回 `false` 时清空该行为
  - 通过 `TryGetActor()` 查找行动点达到上限的角色，按顺序处理全部可行动者
    - 角色行动前会调用 `CombatNode.GetCharacterNode()`，并通过 `MoveScope()`、`ExpandScope()` 播放位移与强调动画
    - 播报“X 的回合”后，根据阵营选择 `PlayerInput` 或 `AIInput` 获取行动，期间 `Combat.Considering` 指向当前角色
    - 等待决策结束后调用 `StartTask()`，立即扣除前摇并触发具体演出
  - 若本帧无人可行动，则等待 `Task.Delay(100)`，将 `Time` 增加 `0.1`，同时为所有存活角色按 `speed.value * 0.1` 恢复行动点
- 战斗循环没有人工上限，只要双方仍有角色存活就会持续执行；任何异常都会被捕获并写入日志，同时结束战斗等待器以防主流程卡死

## 行动决策

- `CombatInput` 是输入基类，提供阵营、目标筛选与界面辅助方法
- `PlayerInput` 的决策步骤：
  - 先弹出仅包含“攻击”的 `MenuDialogue`，为扩展其他指令保留入口
  - 取得敌方存活单位生成菜单，并允许通过返回选项回到上一层
  - 选择敌人后，以同样方式列出该敌人所有可攻击部位，并展示实时生命值
  - 返回 `Attack` 行动实例，战斗主循环会在 `StartTask()` 中立即扣除前摇成本
- `AIInput` 随机挑选敌方角色与身体部位：
  - 若无法找到存活敌人或可攻击部位会抛出异常，主循环会捕获并记录
- 所有菜单交互均由 `DialogueManager` 托管，MCP 模式下可通过 `game_select_option` 指令驱动

## 行动前后摇

- `CombatAction` 抽象类封装统一的前后摇机制，构造函数中的两个浮点参数分别表示前摇与后摇成本
- `StartTask()` 会立即扣除前摇行动点，并执行 `OnStartTask()`，用于前摇动画或提示
- `UpdateTask()` 每帧检查行动点是否重新蓄满，若满足会扣除后摇成本并调用 `OnExecute()`
- `Attack` 的执行过程：
  - 创建 `GenericDialogue` 描述攻击动作，并等待文字打印完成
  - 计算 1~3 点伤害写入被选中部位的生命值，界面通过 `CharacterNode.Shake()` 与受击音效反馈
  - 持续追加部位状态、倒地提示，并在对话框关闭后额外扣除 5 点行动点
- 外部随时可以将 `character.combatAction` 置空以中断后摇，例如强制结束战斗

## 角色与属性

- `Character` 拥有：
  - `PropertyInt speed`：决定每帧行动点回复量，默认最大值为 5
  - `PropertyDouble actionPoint`：记录当前与最大行动点，默认上限为 10
  - 六个 `BodyPart`：分别对应头、双臂、躯干与双腿，并统一暴露在 `bodyParts` 列表中
  - `combatAction`：指向当前执行中的战斗行为
- 角色被视为存活的条件是头部与躯干仍可用，`IsAlive` 会据此返回布尔值
- `BodyPart` 维护独立生命值，通过 `TargetName` 提供中文部位名称，并实现 `ICombatTarget` 接口以统一命中逻辑
- 角色与部位均支持二进制序列化，内部借助 `ReadScope()` 与 `WriteScope()` 保证数据块长度安全

## 装备

- `Equipment` 同样实现 `ICombatTarget`，可在战斗流程中与身体部位一致地被选择或结算伤害
- 每件装备包含名称、`EquipmentType` 掩码与耐久 `HitPoint`；默认情况下耐久度为 10/10，可在构造时自定义
- 装备数据支持二进制序列化，使用 `ReadScope()`、`WriteScope()` 保持与角色存档格式一致

## 战斗界面

- `CombatNode` 提供战斗 UI 框架：
  - `PlayerTeamContainer` 与 `EnemyTeamContainer` 分别展示我方与敌方
  - `Initialize()` 会清空旧节点、实例化新的 `CharacterNode` 并记录映射，确保重复进入战斗时状态干净
  - `GetCharacterNode()` 可按角色查找 UI 节点，`GetReadyPosition()` 与 `GetPKPosition()` 提供待命与对战坐标
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
- 当指定 `LaunchArgs.port` 时，`GenericDialogue` 会自动执行无需操作的分支，便于自动化测试

## 自动化与 MCP

- `MenuDialogue`、`ProgramRoot` 与 `CommandHandler` 在等待输入时会调用 `GameServer.McpCheckpoint()`，向 MCP 客户端广播当前状态
- `CommandHandler` 将 `game_select_option` 指令转化为 `MenuDialogue.SelectAndConfirm()`，实现远程操控菜单项
- `GameServer` 使用 `LogListener` 汇总日志，并在下行响应中返回完整文本，便于自动化分析与回放
