# Buff 系统

## 概述

Buff 系统用于为角色或身体部位添加临时或持续的状态效果，如加速、流血、护盾等。当前代码已提供最小化的类型与接口占位，以支持后续扩展。

## 核心类型

### BuffCode 枚举

位于 `Scripts/Combats/BuffCode.cs`。用于标识 Buff 的类型。当前为空枚举，作为后续扩展的占位。

### Buff 类

位于 `Scripts/Combats/Buff.cs`。

- 仅包含只读字段 `code`（类型为 `BuffCode`），用于标识具体 Buff。
- 构造函数接受 `BuffCode`，用于创建 Buff 实例。

## 所有者接口

### IBuffOwner 接口

位于 `Scripts/Combats/IBuffOwner.cs`。用于声明可持有 Buff 的实体（如角色、身体部位或装备）。

- `IReadOnlyList<Buff> Buffs`：返回所有 Buff 列表。
- `void AddBuff(Buff buff)`：添加 Buff。
- `void RemoveBuff(Buff buff)`：移除 Buff。
- `bool HasBuff(BuffCode buffCode)`：检查是否拥有指定类型的 Buff。

建议在需要支持 Buff 的类型上实现该接口，例如未来的 `Character` 或某些 `BodyPart`。

## 设计原则

- 明确职责：Buff 只描述“效果类型”，具体效果由业务层在结算阶段应用（如速度修正、伤害加成、行动点变更）。
- 可组合：实体可同时拥有多个 Buff，效果应以“叠加或覆盖”策略明确处理（由业务层定义）。
- 序列化友好：为持久化存档，Buff 列表应与实体的其他数据一并序列化（当前接口未内置序列化，建议由拥有者统一处理）。
- 非侵入：Buff 不直接改写实体属性，避免隐式状态，所有效果通过显式查询与结算实现。

## 系统间交互

- 战斗循环：在 `Combat` 的行动与受击结算中读取 Buff 列表，按照约定应用效果（如“加速”影响 `speed` 帧恢复、“流血”在每帧或每回合结算扣减生命值）。
- 反应系统：Buff 可影响反应点或可用反应类型（例如“护盾”允许额外格挡）。
- 物品系统：某些装备可在挂载时附带 Buff（如武器出血），通过 `HasBuff` 判定目标状态。

## 使用示例

以下示例展示一个实体实现 `IBuffOwner` 并在战斗结算中读取 Buff：

```csharp
public class Hero : IBuffOwner
{
    private readonly List<Buff> _buffs = new();
    public IReadOnlyList<Buff> Buffs => _buffs;
    public void AddBuff(Buff buff) => _buffs.Add(buff);
    public void RemoveBuff(Buff buff) => _buffs.Remove(buff);
    public bool HasBuff(BuffCode code) => _buffs.Exists(b => b.code == code);
}

// 在某次受击后添加“流血”
// hero.AddBuff(new Buff(BuffCode.Bleeding));

// 在帧结算时按 Buff 应用效果
// if (hero.HasBuff(BuffCode.Haste)) speed.value += 1;
```

## 注意事项

- 当前 `BuffCode` 未定义具体枚举项，建议在确定效果语义后集中补充，如 `Bleeding`、`Haste`、`Shield` 等。
- 建议将 Buff 的生效时机与持续时间纳入统一约定（按帧、按回合或按事件触发）。
- 若 Buff 会影响序列化，请在拥有者的存档结构中记录 Buff 列表，并为未来版本兼容预留扩展位。

