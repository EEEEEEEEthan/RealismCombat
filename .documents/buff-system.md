# Buff 系统

## 概述

Buff 系统用于在战斗过程中为目标（身体部位或装备）附加临时状态，例如束缚或擒拿。这些状态会在 UI 中展示来源与类型，并在战斗结束时统一清理。

## 核心类型

- `Buff`：位于 `Scripts/Combats/Buff.cs`
  - `BuffCode code`：Buff 类型枚举
  - `BuffSource? source`：来源，包含 `Character Character` 与 `ICombatTarget Target`
- `BuffCode`：位于 `Scripts/Combats/Buff.cs`
  - `Restrained`（束缚）
  - `Grappling`（擒拿）
  - 扩展属性 `Name` 返回中文显示名称
- `IBuffOwner`：位于 `Scripts/Combats/IBuffOwner.cs`
  - `List<Buff> Buffs`：可变 Buff 列表（`BodyPart`、`Item` 等实现均复用）

## 实现位置

- `BodyPart`（`Scripts/Characters/BodyPart.cs`）实现 `IBuffOwner`，允许在身体部位上附加 Buff
- `Item`（`Scripts/Items/Item.cs`）实现 `IBuffOwner`，允许在装备和嵌套装备上附加 Buff

## 施加与清理

- 施加：`GrabAttack` 命中后必然施加（`Scripts/Combats/CombatActions/GrabAttack.cs`）
  - 攻击者手臂添加 `Grappling`，来源为被抓角色与被抓部位/物品
  - 目标对象添加 `Restrained`，来源为攻击者与抓取使用的手臂
  - 抓取伤害为 0；在同一 `GenericDialogue` 内提示“擒拿/束缚”获得，避免重复创建对话框
- 展示：在玩家选择目标或反应时，UI 描述中列出 Buff（`Scripts/Combats/CombatInput.cs`）
  - 显示格式：`[中文类型]来自{来源角色}-{来源目标名称}`
- 清理：
  - 放手：`ReleaseAction` 仅处理自身手臂的擒拿，移除对应 `Grappling`，并按来源匹配移除目标的 `Restrained`，可顺带丢弃该手臂武器
  - 战后：`Combat` 统一清理，遍历双方角色 → 身体部位 → 槽位装备 → 装备嵌套

## 序列化

- 当前 `Item` 的序列化会写入槽位与自定义数据，但 Buff 列表以数量 `0` 写入，反序列化时也忽略 Buff 数据
- 若需要将 Buff 序列化到存档，可扩展 `Item` 与 `BodyPart` 的读写逻辑，并为 `Buff` 定义序列化格式（包含 `code` 与 `source` 标识）

## 使用示例

```csharp
// 施加束缚到目标躯干
var restrained = new Buff(BuffCode.Restrained, actor);
((IBuffOwner)target.torso).AddBuff(restrained);

// 检查是否被擒拿
if (bodyPart.HasBuff(BuffCode.Grappling)) {
    // 调整可用行动或提示
}
```

## 设计注意

- Buff 应尽量为“数据化”的状态，由具体行动或系统在结算时解释其效果
- 清理策略应在战斗结束、存档、或场景切换时统一执行，避免状态残留
- 当 Buff 会影响行动可用性或数值时，优先通过查询接口（如 `HasBuff`）在行为逻辑处做判断，保持模块解耦

