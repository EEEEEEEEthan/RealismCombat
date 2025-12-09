/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 1, 4)
{
	public override CombatActionCode Id => CombatActionCode.Charge;
	public override string Narrative => "用躯干蓄力撞击目标，造成特殊近战攻击";
	public override double DodgeImpact => 0.4;
	public override double BlockImpact => 0.45;
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	public override string PreCastText => $"{actor.name}肩膀下沉...";
	public override string CastText => $"{actor.name}用肩膀撞击{target!.name}的{targetObject!.Name}!";
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Torso, };
}
