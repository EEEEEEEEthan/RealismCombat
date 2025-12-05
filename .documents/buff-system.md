# Buff 系统

## 概述

Buff 系统用于在战斗过程中为目标（身体部位或装备）附加临时状态，例如束缚或擒拿。这些状态会在 UI 中展示来源与类型，并在战斗结束时统一清理。

## 核心类型

- `Buff`：位于 `Scripts/Combats/Buff.cs`，包含：
  - `BuffCode code`：Buff 类型枚举
  - `Character? source`：施加该 Buff 的来源角色，可为空
- `BuffCode`：位于 `Scripts/Combats/BuffCode.cs`，当前实现：
  - `Restrained`（束缚）
  - `Grappling`（擒拿）
  - 扩展方法 `BuffCodeExtensions.GetName()` 返回中文显示名称
- `IBuffOwner`：位于 `Scripts/Combats/IBuffOwner.cs`，可被附加 Buff 的统一接口：
  - `IReadOnlyList<Buff> Buffs`：只读 Buff 列表
  - `AddBuff(Buff buff)` / `RemoveBuff(Buff buff)`：添加/移除 Buff
  - `HasBuff(BuffCode code)`：判断是否包含某类型 Buff

## 实现位置

- `BodyPart`（`Scripts/Characters/BodyPart.cs`）实现 `IBuffOwner`，允许在身体部位上附加 Buff
- `Item`（`Scripts/Items/Item.cs`）实现 `IBuffOwner`，允许在装备和嵌套装备上附加 Buff

## 施加与清理

- 施加：`GrabAttack` 在命中时有 50% 概率施加 Buff（`Scripts/Combats/CombatActions/GrabAttack.cs`）：
  - 对目标躯干添加 `Restrained`
  - 对攻击者使用的身体部位添加 `Grappling`
- 展示：在玩家选择目标或反应时，UI 会在描述中列出 Buff（`Scripts/Combats/CombatInput.cs`）：
  - 显示格式：`[中文类型]来自{来源角色或"未知"}`
- 清理：战斗结束后统一清理 Buff（`Scripts/Combats/Combat.cs`）：
  - 遍历双方角色 → 身体部位 → 槽位装备 → 装备的嵌套槽位
  - 对实现 `IBuffOwner` 的目标调用 `RemoveBuff`

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

