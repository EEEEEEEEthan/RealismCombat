/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat)
{
	internal override double DodgeImpact => 0.4;
	internal override double BlockImpact => 0.45;
	internal override AttackTypeCode AttackType => AttackTypeCode.Special;
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => bodyPart.id == BodyPartCode.Torso;
	public static new bool CanUse(BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart);
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => CanUse(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}撞击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
