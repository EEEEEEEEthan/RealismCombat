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
  - 若本帧无人可行动，则等待 `Task.Delay(100)`，将 `Time` 增加 `0.1`，同时为所有存活角色按 `Speed * 0.1` 恢复行动点；`Speed` 由基础速度 5 乘以负重曲线得到，裸装总重量 75 时为 5，负重越高越接近 5/3
- 战斗循环没有人工上限，只要双方仍有角色存活就会持续执行；任何异常都会被捕获并写入日志，同时结束战斗等待器以防主流程卡死

## 行动决策

- `CombatInput` 是输入基类，提供阵营、目标筛选与界面辅助方法
- `PlayerInput` 的决策步骤：
  - 先列出自身所有可用身体部位生成菜单，并展示实时生命值与当前 Buff，首层菜单不提供返回选项
  - 选择自身身体部位后，根据该部位可用的攻击类型生成菜单（如斩击、刺击、踢、头槌、撞击、抓取等），每个攻击选项的描述首行会显示伤害预览，格式为`伤害 砍X 刺Y 钝Z`
  - 若该部位无法使用任何攻击类型，会提示并返回重新选择
  - 选择攻击类型后，取得敌方存活单位生成菜单，并允许通过返回选项回到上一层
  - 选择敌人后，以同样方式列出该敌人所有可攻击部位，并展示实时生命值，允许通过返回选项回到上一层
  - 返回对应的攻击行动实例（如 `SlashAttack`、`StabAttack` 等），战斗主循环会在 `StartTask()` 中立即扣除前摇成本
- 菜单标题采用导航格式，按层级串联：`{角色名}的回合` → `{角色名}的{己方部位}` → `{角色名}的{己方部位}{行动名}` → `{角色名}的{己方部位}{行动名}{目标角色名}的`，便于玩家与 MCP 日志快速辨识当前决策深度
- `AIInput` 的决策步骤：
  - 随机选择自身一个可用身体部位
  - 根据该部位可用的攻击类型随机选择一个
  - 若该部位无法使用任何攻击类型，会尝试选择下一个身体部位
  - 随机挑选敌方角色与身体部位
  - 若无法找到可用身体部位、可用攻击类型、存活敌人或可攻击部位会抛出异常，主循环会捕获并记录
- 所有菜单交互均由 `DialogueManager` 托管，MCP 模式下可通过 `game_select_option` 指令驱动

## 行动前后摇

- `CombatAction` 抽象类封装统一的前后摇机制，构造函数中的两个浮点参数分别表示前摇与后摇成本
- `StartTask()` 会立即扣除前摇行动点，并执行 `OnStartTask()`，用于前摇动画或提示
- `UpdateTask()` 每帧检查行动点是否重新蓄满，若满足会扣除后摇成本并调用 `OnExecute()`
- `AttackBase` 是所有攻击类型的基类，前后摇成本由具体攻击在构造时传入，不再固定为 3/3
- 攻击类的前后摇配置：
  - 斩击：前摇 4、后摇 2
  - 刺击：前摇 2、后摇 4
  - 踢击：前摇 3、后摇 3
  - 头槌：前摇 2、后摇 4
  - 撞击：前摇 4、后摇 2
  - 抓取：前摇 2、后摇 4
- `AttackBase` 构造函数接受攻击者、攻击者身体部位、目标角色、目标身体部位与战斗实例
- `AttackBase` 包含 `ActorBodyPart` 属性，记录攻击者使用的身体部位
- `AttackBase.Description` 已内联描述组装逻辑，展示攻击类型、闪避倾向、格挡倾向与子类 `Narrative` 提供的叙述文本；子类仅需实现 `Narrative`
- `AttackBase` 的执行过程：
  - 创建 `GenericDialogue` 描述攻击动作（通过 `GetStartDialogueText()` 获取文本，通常显示"X抬起Y开始蓄力..."），并等待文字打印完成
  - 创建 `GenericDialogue` 描述攻击命中（通过 `GetExecuteDialogueText()` 获取文本，如"X用Y斩击Z的W!"），并等待文字打印完成
  - 在攻击命中前调用 `Combat.HandleIncomingAttack()` 处理受击方的反应决策
  - 根据反应类型执行不同逻辑：
    - 格挡：伤害转移到格挡目标（身体部位或装备），播放格挡音效与动画
    - 闪避：攻击落空，受击方打断当前行动并位移
    - 承受：正常结算伤害
  - 通过 `CalculateDamage()` 计算伤害值（通常为 1~3 点）写入最终目标的生命值，界面通过 `CharacterNode.Shake()` 与受击音效反馈
  - 注意：`GrabAttack`（抓取攻击）不造成伤害，`CalculateDamage()` 返回 0
  - 持续追加部位状态、倒地提示，并在对话框关闭后额外扣除 5 点行动点
- `AttackBase.CalculateDamage()` 直接内联原 `DamageResolver.GetBaseDamage()` 逻辑：若动作使用武器，则遍历攻击部位槽位取首个 `ItemFlagCode.Arm` 武器的 `DamageProfile.Get(AttackType)`，否则 `Special` 返回 `Damage(0,0,1)`、其余返回 `Damage.Zero`，最后统一乘以 `DamageMultiplier`
- 外部随时可以将 `character.combatAction` 置空以中断后摇，例如强制结束战斗

## 攻击类型

- `AttackBase` 是攻击的抽象基类，位于 `Scripts/Combats/CombatActions/AttackBase.cs`
- 所有攻击类型都继承自 `AttackBase`，并实现以下抽象方法：
  - `GetStartDialogueText()`：返回攻击开始时的对话文本
  - `GetExecuteDialogueText()`：返回攻击执行时的对话文本
  - `CalculateDamage()`：计算并返回伤害值（返回 `Damage`，包含劈砍/穿刺/钝击三项）
- 攻击可用性通过实例方法检查：抓取要求手臂可用且 `!bodyPart.HasWeapon`，斩击/刺击要求手臂可用且 `bodyPart.HasWeapon`，基类不再提供静态 `HasWeapon`
- 当前实现的攻击类型：
  - `SlashAttack`（Swing 挥砍）：只允许有武器的手臂使用
  - `StabAttack`（Thrust 捅扎）：只允许有武器的手臂使用，闪避影响 0.7、格挡影响 0.35，相比其他攻击更容易被闪避且更难被格挡
  - `KickAttack`（Special 特殊）：只允许腿使用
  - `HeadbuttAttack`（Special 特殊）：只允许头使用
  - `ChargeAttack`（Special 特殊）：只允许躯干使用
  - `GrabAttack`（抓取）：只允许没有武器的手臂使用，不造成伤害，主要用于施加束缚和擒拿 buff
- `GetAvailableAttacks()` 方法会根据身体部位返回所有可用的攻击类型列表

## 其他行动

- `BreakFreeAction`（抽出）：当行动部位或其装备存在`束缚` Buff 时可用；前摇 2、后摇 1 行动点；执行时有 50% 概率移除找到的束缚，失败会提示未能挣脱
- `TakeWeaponAction`（拿）：当手部有空槽且腰带上挂有武器时可用；从当前已装备的腰带槽位将武器移动到该手部；若手上已有武器会直接将旧武器丢入战斗的 `droppedItems` 集合后再放入新武器；前摇 2、后摇 1 行动点；玩家和 AI 均会在行动菜单中出现“拿”选项；执行不造成伤害，仅搬运装备。
- `ReleaseAction`（放手）：当双臂存在擒拿 Buff 或手上握有武器时可用；若身上存在擒拿 Buff，会解除自身施加的束缚并提示被解放的目标数量；若仅握有武器，会在前摇阶段立即移除手中符合武器槽 (`ItemFlagCode.Arm`) 的物品并提示“丢下了{部位}的{武器名}”，不再出现“准备丢下”提示；AI 仅在需要解除擒拿时考虑该行动，避免无故把武器丢弃。
- `PickWeaponAction`（捡）：当双臂中存在空的武器槽且战斗的 `droppedItems` 中有与该槽匹配的武器时可用；玩家会列出可拾取的武器并选择其一，AI 随机选择；执行时从 `droppedItems` 移除被捡起的武器，若手上已有物品会先放回 `droppedItems` 再装备新武器；前摇 8、后摇 3 行动点，执行不造成伤害，仅拾取搬运。
- 目标与格挡菜单的描述通过公共方法组装，统一展示生命值与 Buff 列表。

## 反应系统

- 每个角色拥有 `reaction` 属性，表示当前可用的反应点数，默认值为 1；战斗开始时会被重置为 0，轮到该角色行动前重新设置为 1
- 当角色受到攻击时，若 `reaction > 0`，会通过 `CombatInput.MakeReactionDecisionTask()` 获取反应决策；`reaction <= 0` 时 AI 直接承受，玩家界面中格挡与闪避选项会灰化不可用，仅保留承受
- AI 在有反应点时有 25% 概率直接选择承受，不额外执行格挡或闪避
- 反应类型包括：
  - 格挡（Block）：消耗 1 点反应，选择身体部位或装备承受伤害，伤害会转移到格挡目标
  - 闪避（Dodge）：消耗 1 点反应，打断自身当前行动并躲开攻击，攻击完全落空
  - 承受（Endure）：不消耗反应，正常承受伤害
- 发动格挡或闪避都会清除自身当前的 `combatAction`（如蓄力中的攻击会被中断）
- `PlayerInput` 的反应决策：
  - 弹出菜单让玩家选择反应类型
  - 选择格挡时会列出所有可用的格挡目标（可用身体部位与装备）
  - 若没有可用格挡目标会提示并重新选择
- 玩家反应菜单标题格式为“应对{攻击者武器/部位}对{防守者部位}的{攻击名}”，例如“应对贵族兵长剑对Ethan头部的斩击”；防守方部位标题仅显示裸部位名，不再拼接装备标签
- 反应菜单中文案：格挡选项展示“成功率XX%”对应本次格挡判定成功率，闪避选项展示“成功率XX%”对应本次闪避判定成功率
- `AIInput` 的反应决策：
  - 优先使用装备进行格挡，若无装备则选择第一个可用身体部位
  - 若无可用格挡目标则选择承受
- `GetBlockTargets()` 方法会返回角色的所有可用身体部位，以及双臂装备的可用装备
- 角色每回合开始时会将 `reaction` 重置为 1
- 闪避与格挡成功率：
  - 由 `ReactionSuccessCalculator` 以 sigmoid 公式计算，输入包含武器长度(0~2归一)降低闪避、提高格挡；武器重量(0~2归一)同时提高闪避与格挡；动作自带闪避/格挡影响系数(0~1)；防守方体重70加装备重量越大成功率越低；徒手攻击附加惩罚
  - 判定结果在 `AttackBase.OnExecute` 中结算，反应提示仅展示成功或失败，不再显示当次成功率
  - 选择攻击目标时，`PlayerInput` 预先展示该次攻击对每个可攻击部位的闪避/格挡成功率，便于战前判断
  - 躯干与裆部格挡判定额外+0.35加成
- 各攻击的 `DodgeImpact`/`BlockImpact` 会根据攻击部位与目标部位的归一化高度差调整：`BodyPartCode.NormalizedHeight` 取值头1.0、躯干0.85、裆0.7、手臂0.75、腿0.35，差值≥0.4时大幅提高被闪避/格挡概率，用于表现头槌打脚、脚踢头等极端角度更易被防御

## 命中与护甲覆盖

- 伤害结算前在 `AttackBase.OnExecute()` 内联决定最终受击目标与防护值（原先的 `DamageResolver.ResolveTarget()` 已去除）
- 若目标为身体部位，会收集该部位上所有护甲（TorsoArmor/HandArmor/LegArmor），逐件按随机起始顺序检验 `Coverage`；命中则使用该护甲的 `Protection`，未命中继续下一件
- 若所有护甲都未命中，则直接命中部位，防护为 0
- 若目标本身是装备，直接使用装备自带的 `Protection`
- 格挡成功时同样走上述流程：若格挡目标是身体部位使用其护甲覆盖率，若是装备则直接对该装备结算，不做覆盖判定

## 角色与属性

- `Character` 拥有：
  - `PropertyInt speed`：决定每帧行动点回复量，默认最大值为 5
  - `PropertyDouble actionPoint`：记录当前与最大行动点，默认上限为 10
  - 六个 `BodyPart`：分别对应头、双臂、躯干与双腿，并统一暴露在 `bodyParts` 列表中
  - `combatAction`：指向当前执行中的战斗行为
  - `AvailableCombatTargets`：返回当前可用的 `ICombatTarget` 数组，供攻击流程直接使用实例属性获取目标
- 角色被视为存活的条件是头部与躯干仍可用，`IsAlive` 会据此返回布尔值
- `BodyPart` 维护独立生命值，通过 `Name` 属性提供中文部位名称（通过扩展方法 `GetName()` 实现），并实现 `ICombatTarget` 接口以统一命中逻辑
- 角色与部位均支持二进制序列化，内部借助 `ReadScope()` 与 `WriteScope()` 保证数据块长度安全

## 攻击类型与伤害类型

- `AttackTypeCode` 定义攻击动作的类型，位于 `Scripts/Combats/AttackTypeCode.cs`：
  - `Slash`：劈砍
  - `Pierce`：穿刺
  - `Special`：特殊
- `DamageTypeCode` 定义造成的伤害类型，位于 `Scripts/Combats/DamageTypeCode.cs`：
  - `Slash`：劈砍
  - `Pierce`：穿刺
  - `Blunt`：钝击
- 攻击类型与伤害类型是不同的概念：攻击类型描述攻击动作的特性，伤害类型描述造成的伤害特性

## 装备

- `Item` 抽象类实现 `ICombatTarget`，可在战斗流程中与身体部位一致地被选择或结算伤害
- 每件装备包含 `ItemIdCode` 标识、`ItemFlagCode` 掩码（用于匹配装备槽）与耐久 `HitPoint`
- 装备通过 `ItemSlot` 挂载到身体部位，`BodyPart` 的 `Slots` 数组管理装备槽
- 装备槽通过 `ItemFlagCode` 匹配，只有标志匹配的装备才能放入对应槽位
- 默认情况下装备耐久度为 10/10，可在构造时自定义
- 装备数据支持二进制序列化，使用 `ReadScope()`、`WriteScope()` 保持与角色存档格式一致
- 当前实现中，双臂部位默认拥有 `ItemFlagCode.Arm` 类型的装备槽，可用于装备武器

## 战斗界面

- `CombatNode` 提供战斗 UI 框架：
  - `PlayerTeamContainer` 与 `EnemyTeamContainer` 分别展示我方与敌方
  - `Initialize()` 会清空旧节点、实例化新的 `CharacterNode` 并记录映射，确保重复进入战斗时状态干净
  - `GetCharacterNode()` 可按角色查找 UI 节点，`GetReadyPosition()` 与 `GetPKPosition()` 提供待命与对战坐标
- `CharacterNode` 展示角色信息：
  - `NameLabel` 显示角色名称
  - `PropertyNode` 显示行动点与生命值，`Jump` 属性用于表现行动状态
  - 提供 `MoveTo()`、`Shake()` 等动画接口，可在后续加入更多战斗表现
- `PropertyNode` 内置 Shader 实现"跳动"效果，通过 `Jump` 开关控制，便于提示正在行动的角色
- `BleedingNode` 提供流血动画效果：
  - 继承自 `TextureRect`，用于显示角色受伤时的流血动画
  - 使用 `SpriteTable.Bleeding` 中的动画帧，共 11 帧
  - 在 `_Process()` 中根据随机间隔切换动画帧，实现不规则闪烁效果
  - 当节点可见性改变时，会重置动画索引和时间，确保动画连贯
  - 时间间隔在 0 到 0.3 秒之间随机，增加视觉真实感

## 战斗入口

- 在 `Game` 菜单选择“开始战斗”时，项目会：
  - 从 `ResourceTable` 实例化 `CombatNode`
  - 创建 `Combat` 并传入 `Game` 持有的玩家角色列表，与默认敌人编队对战
  - 玩家角色列表在 `Game` 生命周期内保持复用，并在存档时写入文件
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

## 战后收尾

- `Combat` 构造时会记录所有已装备物品与其所属槽位（含嵌套容器）
- 结束战斗调用 `EndBattle()`：优先尝试将物品归还原槽；若槽位已被占用，会将占位物品移入物品栏后再放回原物；对于已不在角色身上的容器不会强行归还
- 收尾阶段同时清除全部 Buff，并设置战斗等待结果；异常路径也会走 `EndBattle()`，避免装备状态丢失
