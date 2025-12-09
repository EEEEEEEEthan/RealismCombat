/// <summary>
///     头槌攻击，只允许头使用
/// </summary>
public class HeadbuttAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Code => CombatActionCode.Headbutt;
	public override string Narrative => "用头部进行头槌攻击，近距离的特殊冲撞";
	internal override double DodgeImpact => 0.35;
	internal override double BlockImpact => 0.35;
	internal override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Head, };
	protected override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}头槌{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
