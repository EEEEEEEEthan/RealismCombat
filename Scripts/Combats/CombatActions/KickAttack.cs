using Godot;
/// <summary>
///     踢击攻击，只允许腿使用
/// </summary>
public class KickAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat)
{
	public override CombatActionCode Code => CombatActionCode.Kick;
	public override string Description => BuildAttackDescription("用腿部发动踢击，依赖腿部可用性进行近战攻击");
	internal override double DodgeImpact => 0.6;
	internal override double BlockImpact => 0.4;
	internal override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart.Available && IsLeg(bodyPart.id);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}踢击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
