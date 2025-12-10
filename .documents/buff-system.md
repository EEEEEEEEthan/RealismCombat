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
  - `Bleeding`（流血）
  - `Prone`（倒伏）
  - 扩展属性 `Name` 返回中文显示名称
- `IBuffOwner`：位于 `Scripts/Combats/IBuffOwner.cs`
  - `List<Buff> Buffs`：可变 Buff 列表（`BodyPart`、`Item` 等实现均复用）

## 实现位置

- `BodyPart`（`Scripts/Characters/BodyPart.cs`）实现 `IBuffOwner`，允许在身体部位上附加 Buff
- `Item`（`Scripts/Items/Item.cs`）实现 `IBuffOwner`，允许在装备和嵌套装备上附加 Buff

## 施加与清理

- 施加：
  - `GrabAttack` 命中后必然施加（`Scripts/Combats/CombatActions/GrabAttack.cs`）
    - 攻击者手臂添加 `Grappling`，来源为被抓角色与被抓部位/物品
    - 目标对象添加 `Restrained`，来源为攻击者与抓取使用的手臂
    - 抓取伤害为 0；在同一 `GenericDialogue` 内提示“擒拿/束缚”获得，避免重复创建对话框
  - `ChargeAttack` 施加倒伏有两种情况（`Scripts/Combats/CombatActions/ChargeAttack.cs`）：
    - 攻击目标为腿部时，无论是否命中，攻击者躯干必然添加 `Prone`，来源为攻击者与目标腿部
    - 攻击命中时，按重量比概率给目标躯干添加 `Prone`，概率为 targetWeight / actorWeight，来源为攻击者与攻击者躯干；攻击者越重、目标越轻时倒伏概率越低
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

## Buff 效果

### 倒伏 (Prone)

- 当角色处于倒伏状态时，除了爬起行动 (`GetUpAction`) 外的所有行动都会被禁用
- 实现位置：`Scripts/Combats/CombatActions/CombatAction.cs` 中的 `DisabledByBuff` 属性
- 检查逻辑：遍历角色所有身体部位，只要有任意部位存在倒伏 Buff，且当前行动不是 `GetUpAction`，则禁用该行动
- 倒伏状态通过 `GetUpAction` 解除，该行动会清除角色所有身体部位上的倒伏 Buff
- 倒伏状态的文字提示：
  - 躯干撞击腿部时：攻击者必定获得倒伏，显示"[角色名]撞击腿部导致自己失去平衡倒下了!"（`Scripts/Combats/CombatActions/ChargeAttack.cs`）
  - 躯干撞击命中时：目标角色按重量比概率倒伏，显示"[角色名]失去平衡倒下了!"（`Scripts/Combats/CombatActions/ChargeAttack.cs`）
  - 头槌腿部时：攻击者必定倒伏，显示"[角色名]头槌腿部导致自己失去平衡倒下了!"（`Scripts/Combats/CombatActions/HeadbuttAttack.cs`）
  - 头槌手臂或躯干时：目标按重量比概率倒伏，显示"[角色名]被头槌撞倒了!"（`Scripts/Combats/CombatActions/HeadbuttAttack.cs`）

### 束缚 (Restrained)

- 被束缚的部位除了抽出行动 (`BreakFreeAction`) 外的所有行动都会被禁用
- 被束缚部位不能用于格挡
- 被束缚部位的装备槽位中的目标也不能用于格挡
- 被束缚的是腿时，闪避概率降至 1/5

### 擒拿 (Grappling)

- 存在擒拿 Buff 的部位除了放手行动 (`ReleaseAction`) 外的所有行动都会被禁用
- 角色任意部位存在擒拿或束缚状态时，闪避和格挡成功率下降到 1/3

### 流血 (Bleeding)

- 每 5 个 tick，对存在流血 Buff 的角色随机选择一个身体部位，令其生命值 -1
- 角色任意部位有流血 Buff 时，角色节点会播放流血动画
- 身体部位受到砍伤时必定造成流血，受到刺伤时 50% 概率造成流血
- 当身体部位首次获得流血 Buff 时，会显示文字提示："[角色名]的[部位名]开始流血!"
  - 实现位置：`Scripts/Combats/CombatActions/AttackBase.cs` 中的伤害处理逻辑

## 设计注意

- Buff 应尽量为"数据化"的状态，由具体行动或系统在结算时解释其效果
- 清理策略应在战斗结束、存档、或场景切换时统一执行，避免状态残留
- 当 Buff 会影响行动可用性或数值时，优先通过查询接口（如 `HasBuff`）在行为逻辑处做判断，保持模块解耦
- **防止重复添加**：在添加 Buff 前必须先检查同 id 的 Buff 是否已存在，避免重复施加相同状态。所有添加 Buff 的地方都应调用 `HasBuff(BuffCode, false)` 进行检查


````

