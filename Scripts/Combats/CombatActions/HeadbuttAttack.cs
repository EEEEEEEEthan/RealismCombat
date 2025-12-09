/// <summary>
///     头槌攻击，只允许头使用
/// </summary>
public class HeadbuttAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Id => CombatActionCode.Headbutt;
	public override string Narrative => "用头部进行头槌攻击，近距离的特殊冲撞";
	public override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}头槌{target.name}的{targetObject.Name}!";
	public override double DodgeImpact => 0.35;
	public override double BlockImpact => 0.35;
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id: BodyPartCode.Head, };
}
