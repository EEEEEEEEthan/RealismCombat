/// <summary>
///     斩击攻击，只允许有武器的手臂使用
/// </summary>
public class SlashAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 4, 2)
{
	public override CombatActionCode Id => CombatActionCode.Slash;
	public override string Narrative => "持武器挥砍目标，造成挥砍伤害，依赖手部武器";
	public override string PreCastText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string CastText => $"{actor.name}用{actorBodyPart.Name}斩击{target.name}的{targetObject.Name}!";
	public override double DodgeImpact => 0.45;
	public override double BlockImpact => 0.65;
	public override AttackTypeCode AttackType => AttackTypeCode.Swing;
	public override bool UsesWeapon => true;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsArm: true, HasWeapon: true, };
}
