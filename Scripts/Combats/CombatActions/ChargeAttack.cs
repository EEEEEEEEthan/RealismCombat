/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 4, 2)
{
	public override CombatActionCode Code => CombatActionCode.Charge;
	public override string Description => BuildAttackDescription("用躯干蓄力撞击目标，造成特殊近战攻击");
	internal override double DodgeImpact => 0.4;
	internal override double BlockImpact => 0.45;
	internal override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart.Available && bodyPart.id == BodyPartCode.Torso;
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}撞击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
