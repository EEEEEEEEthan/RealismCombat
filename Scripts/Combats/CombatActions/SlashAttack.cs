using Godot;
/// <summary>
///     斩击攻击，只允许有武器的手臂使用
/// </summary>
public class SlashAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat)
{
	public override string Description => BuildAttackDescription("持武器挥砍目标，造成挥砍伤害，依赖手部武器");
	internal override double DodgeImpact => 0.45;
	internal override double BlockImpact => 0.65;
	internal override AttackTypeCode AttackType => AttackTypeCode.Swing;
	internal override bool UsesWeapon => true;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart.Available && IsArm(bodyPart.id) && HasWeapon(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}斩击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
