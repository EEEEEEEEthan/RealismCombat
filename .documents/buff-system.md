# Buff 系统

## 概述

Buff 系统用于为角色或装备施加临时状态效果，用以表达“束缚”“擒拿”等战斗中的持续影响。系统由 `Buff`、`BuffCode` 与 `IBuffOwner` 三部分组成，并由 `BodyPart` 与 `Item` 等目标实现与承载。

## 核心类型

### Buff

- 位置：`Scripts/Combats/Buff.cs`
- 字段：`BuffCode code`、`Character? source`
- 作用：描述一个具体的 Buff 实例与其来源角色（可为空）

### BuffCode 与扩展

- 位置：`Scripts/Combats/BuffCode.cs`
- 枚举项：`Restrained`（束缚）、`Grappling`（擒拿）
- 扩展方法：`BuffCodeExtensions.GetName()` 返回中文显示名

### IBuffOwner

- 位置：`Scripts/Combats/IBuffOwner.cs`
- 能力：`IReadOnlyList<Buff> Buffs`、`AddBuff(Buff)`、`RemoveBuff(Buff)`、`HasBuff(BuffCode)`
- 作用：统一 Buff 托管接口，任何可被施加状态的对象均可实现

## 实现者与承载

### BodyPart（身体部位）

- 位置：`Scripts/Characters/BodyPart.cs`
- 实现：`IBuffOwner`，内部维护 `List<Buff>` 并暴露 `Buffs`
- 典型用途：在抓取命中时，对 `torso`（躯干）施加 `Restrained`（束缚）以表示被控制状态

### Item（装备）

- 位置：`Scripts/Items/Item.cs`
- 实现：`IBuffOwner`，允许为装备施加状态（如被擒拿的手臂装备）
- 序列化：当前实现中物品的 Buff 计数会写入占位值，具体 Buff 数据暂未持久化，后续版本可扩展

## 交互与应用

### 抓取攻击（GrabAttack）

- 位置：`Scripts/Combats/CombatActions/GrabAttack.cs`
- 命中逻辑：
  - 以 50% 概率为目标躯干施加 `Restrained`
  - 为攻击者使用的身体部位施加 `Grappling`
- 效果表达：通过 `resultMessages` 追加提示文本，配合对话框与战斗 UI 呈现

## 使用示例

- 检查状态：`if (bodyPart.HasBuff(BuffCode.Restrained)) { /* 处理被束缚逻辑 */ }`
- 添加状态：`bodyPart.AddBuff(new Buff(BuffCode.Restrained, actor));`
- 移除状态：`bodyPart.RemoveBuff(buff);`

## 设计注意事项

- 状态归属：优先将“被控制类”状态挂在目标部位（如 `torso`），而“动作类”状态挂在施术者的部位（如 `RightArm`）
- 生命周期：Buff 的移除应由具体行动或结算阶段决定，避免永久累积
- 序列化策略：为保证兼容性，推荐在未来版本中为 `BodyPart` 与 `Item` 的 Buff 列表增加稳定的读写格式（可复用 `ReadScope()`/`WriteScope()`）
