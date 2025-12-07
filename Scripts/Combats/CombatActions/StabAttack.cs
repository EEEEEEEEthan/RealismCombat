using Godot;
/// <summary>
///     刺击攻击，只允许持武器的手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat)
{
	internal override double DodgeImpact => 0.55;
	internal override double BlockImpact => 0.5;
	internal override AttackTypeCode AttackType => AttackTypeCode.Thrust;
	internal override bool UsesWeapon => true;
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => IsArm(bodyPart.id);
	public static bool CanUse(BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart) && HasWeapon(bodyPart);
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => CanUse(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}刺击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
