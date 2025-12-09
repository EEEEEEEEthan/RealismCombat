/// <summary>
///     刺击攻击，只允许持武器的手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Id => CombatActionCode.Stab;
	public override string Narrative => "持武器刺击目标，造成刺击伤害，依赖手部武器";
	public override string PreCastText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}刺击{target.name}的{targetObject.Name}!";
	public override double DodgeImpact => 0.7;
	public override double BlockImpact => 0.35;
	public override AttackTypeCode AttackType => AttackTypeCode.Thrust;
	public override bool UsesWeapon => true;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsArm: true, HasWeapon: true, };
}
