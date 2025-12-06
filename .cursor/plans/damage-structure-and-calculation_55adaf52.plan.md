---
name: damage-structure-and-calculation
overview: 引入伤害/防护结构并用动作倍率与武器基础值结算各类型伤害，再按护甲抵消并向上取整扣除目标部位生命
todos:
  - id: data-structs
    content: 新增 Damage/Protection 结构体与运算
    status: pending
  - id: item-config
    content: 为武器护甲填充伤害和防护占位值
    status: pending
  - id: action-multiplier
    content: 为各攻击动作设定类别和倍率
    status: pending
  - id: damage-resolution
    content: 实现按防护结算并扣血
    status: pending
  - id: copy-check
    content: 更新文案/文档并自测
    status: pending
---

# 方案：实现伤害/防护结算

1) 定义数据结构

- 在 `Scripts/Combats/` 新增 `Damage` 与 `Protection` struct：含 `slash/pierce/blunt` 三项，提供构造、加减、Clamp0、乘法(倍率)、总量求和等基础方法，默认值为0。
- 保留伤害类型枚举 `DamageTypeCode`，新增攻击类别枚举映射：`AttackTypeCode.Slash -> Swing` 重命名为 Swing 并保持其他值，刺击改为 Thrust。

2) 武器与护甲基础数值

- 在 `Scripts/Items/Item.cs` 的 `ItemConfig` 中加入 `DamageBase` 和 `Protection` 字段；为现有示例装备填入占位值，满足用户给出的“剑”情景：长剑提供 Swing 与 Thrust 的基础伤害（slash/blunt 或 pierce），护甲类（棉甲/链甲/板甲/护腿等）提供对应 `Protection`。

3) 攻击动作倍率

- 在各攻击类 `Slash(Swing)Attack`、`Stab(Thrust)Attack`、`KickAttack`、`HeadbuttAttack`、`ChargeAttack` 中增加 `AttackTypeCode` 指示类别，并为每个动作定义对三类攻击（Swing/Thrust/Special）各自的倍率；默认以现有伤害模型：剑 Swing/Thrust 乘以 ~1.0 基准，踢/头槌/撞为 Special，基础钝击=1。

4) 伤害计算流程

- 在 `AttackBase.CalculateDamage` 相关实现中改为：获取武器或徒手基础 `Damage`（按攻击类别取基础值）；与动作倍率相乘得到本次 `Damage`；取目标（格挡后的部位或装备）护甲 `Protection` 逐项抵消并 Clamp 至0；若任一分量大于0，对该 `ICombatTarget` 的 HitPoint 扣除 `Ceil(totalDamage)`，并生成描述信息。
- 抽取结算函数（如 `DamageResolver.Resolve(damage, protection)`）以便复用，并确保结果最少0。

5) 文案与类别引用调整

- 将攻击类别展示文本从 Slash 改为 Swing 对应中文“劈砍/挥击”；保持伤害类型中文“劈砍/穿刺/钝击”。更新相关对话字符串与文档 `.documents/combat-system.md` 的攻击类别描述。

6) 测试与自查

- 跑最小自测：持长剑对无甲/棉甲/链甲/板甲目标进行 Swing 与 Thrust，验证伤害输出接近用户给的期望；踢/头槌/撞在有甲时为0，无甲时为1。