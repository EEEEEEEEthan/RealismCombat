/// <summary>
///     抓取攻击，只允许没有武器的手臂使用
/// </summary>
public class GrabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Id => CombatActionCode.Grab;
	public override string Narrative => "徒手擒拿目标，命中可使目标被束缚并让自身进入擒拿状态";
	public override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	public override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}抓取{TargetCharacter.name}的{TargetCombatObject.Name}!";
	public override double DodgeImpact => 0.7;
	public override double BlockImpact => 0.25;
	public override AttackTypeCode AttackType => AttackTypeCode.Special;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart is { Available: true, id.IsArm: true, HasWeapon: false, };
}
