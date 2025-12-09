/// <summary>
///     踢击攻击，只允许腿使用
/// </summary>
public class KickAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 3, 3)
{
	public override CombatActionCode Id => CombatActionCode.Kick;
	public override string Narrative => "用腿部发动踢击，依赖腿部可用性进行近战攻击";
	public override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}踢击{TargetCharacter.name}的{TargetCombatObject.Name}!";
	public override double DodgeImpact => 0.6;
	public override double BlockImpact => 0.4;
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsLeg: true, };
}
