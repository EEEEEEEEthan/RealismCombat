# 半回合制战斗玩法（RealismCombat）

## 定位

复古 JRPG 风格，**半回合制**（非严格轮流出手）：全局时间推进，各单位独立积累**行动力**；谁先到阈值谁先选行动，行动含**前摇 → 执行 → 后摇扣 AP**，形成节奏差与打断空间。

## 核心循环

1. **蓄力（Idle）**  
   每 tick 增加行动力，直至达到上限（满槽）。满槽后玩家/AI 可**宣告**一个行动（进入行动状态）。

2. **前摇（Windup）**  
   宣告后先扣除满槽用的那一拍（进入行动时已消耗「满槽」这一条件），再按该行动的**前摇行动力**逐 tick 消化；前摇未结束前不结算行动的主要效果（攻击命中等）。

3. **执行（Resolve）**  
   前摇归零的当拍（或紧随其逻辑）执行行动效果（伤害、位移、Buff 等，依具体 `Action` 实现）。

4. **后摇（Recovery）**  
   执行后按该行动的**后摇行动力**从当前行动力中**扣除**；单位可能进入低 AP 状态，需重新蓄力。不同行动前后摇不同，体现招式重量感。

## 与代码的对应关系（便于对齐讨论）

| 玩法概念 | 代码中常见说法 |
|----------|----------------|
| 行动力 | `action_points`（`Property`，有 max） |
| 前摇 | `windup` / `get_windup_action_points()`，行动状态内 `_windup` 递减 |
| 后摇 | `recovery` / `get_recovery_action_points()`，执行后从 `action_points` 扣除 |
| 每 tick 回 AP | `character_state_machine_idle_state` |
| 前摇→执行→扣后摇 | `character_state_machine_action_state` |

讨论「行动力满了才能动」「前摇条」「硬直/后摇」时，默认指上表语义。

## 设计约束（给 AI / 新功能）

- **半回合制**：多单位 AP 并行增长，不要求「我方全员动完敌方再动」的纯回合顺序。
- **前摇与后摇都用「行动力单位」计量**，并与 `action_points_per_tick` 换算成 tick（见状态机注释），避免混用「秒」与「格」导致口径不一。
- 扩展新行动时：在对应 `Action` 子类实现 `get_windup_action_points()` / `get_recovery_action_points()`，保持与 UI 中「前后摇」展示一致。

## 延伸阅读

若需与引擎节点生命周期、信号等结合，可配合仓库内 `godot-engine`、`godot-script` 技能；若改战斗易错点，查 `project-pitfalls`。
