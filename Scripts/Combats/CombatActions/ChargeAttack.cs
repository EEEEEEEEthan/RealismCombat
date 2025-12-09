/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 4, 2)
{
	public override CombatActionCode Id => CombatActionCode.Charge;
	public override string Narrative => "用躯干蓄力撞击目标，造成特殊近战攻击";
	public override double DodgeImpact => 0.4;
	public override double BlockImpact => 0.45;
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Torso, };
	public override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}撞击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
