---
name: refactor-combat-action-init
overview: 调整CombatActions构造参数与CombatInput构建流程，改为角色/战斗为构造参数，其他上下文在CombatInput阶段选择并注入，确保可用选项正确呈现。
todos:
  - id: review-apis
    content: 梳理动作构造与依赖调用链
    status: pending
  - id: refactor-constructors
    content: 重构Action构造为actor+combat并增设注入接口
    status: pending
  - id: rebuild-combatinput
    content: 改造CombatInput选择流程填充上下文
    status: pending
  - id: update-canuse
    content: 修正可用性判定与选项展示
    status: pending
  - id: self-test
    content: 自测玩家/AI流程编译通过
    status: pending
---

# 战斗流程重构方案

1) 理解当前Action接口与调用链：阅读 `Scripts/Combats/CombatActions` 下的主要Action构造签名与 `CombatInput` 现有选目标流程，明确bodyPart/target/combatTarget依赖点。
2) 调整Action构造函数：统一保留 `Character actor, Combat combat` 参数，移除 bodyPart/target/combatTarget 等其余构造参数，并提供必要的设置方式（如属性或Init方法）以便后续注入。同步更新派生类引用。
3) 重构 `CombatInput` 构建流程：按“选身体部位→选目标角色→选目标CombatTarget”顺序收集上下文，使用选择结果填充Action实例所需字段，再列出可用Action选项，确保AI/玩家流程兼容。
4) 补充/调整判定与可用性校验：更新 `CanUse` 等方法以适配新构造方式，确保选项展示、禁用逻辑与执行阶段一致。
5) 自测与检查：跑对应逻辑的自测流程（对话选择、AI随机），确认无编译错误、流程正常。

API示例:
public class ActionBase(Combat combat, Character actor)

{

public readonly Character actor;

public readonly Combat combat;

public ICombatTarget ActorObject { get; set; }

public bool CanUse => Available && !Disalbed;

public bool Disabled { get; }  // disabled的项出现在菜单但是是灰色

public bool Available { get; }  // inavailable的项不显示在菜单

public IEnumerable<Character> AvailableTargets { get; }

public Character Target { get; set; }

public IEnumerable<(ICombatTarget, bool disabled)> AvailableTargetObjects { get; }

public ICombatTarget TargetObject { get; set; }

public Task StartTask();

public Task<bool> UpdateTask();

}
战斗过程选身体部位,然后创建所有Action,然后列出列表,选择其中一个.
选目标,选目标身体部位的过程即往这些对象设置值的过程.
全部设置完后走老流程