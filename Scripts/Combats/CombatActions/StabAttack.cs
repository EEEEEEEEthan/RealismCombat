using Godot;
/// <summary>
///     刺击攻击，只允许持武器的手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Combat combat)
	: AttackBase(actor, actorBodyPart, combat, 2, 4)
{
	public override CombatActionCode Code => CombatActionCode.Stab;
	public override string Description => BuildAttackDescription("持武器刺击目标，造成刺击伤害，依赖手部武器");
	internal override double DodgeImpact => 0.7;
	internal override double BlockImpact => 0.35;
	internal override AttackTypeCode AttackType => AttackTypeCode.Thrust;
	internal override bool UsesWeapon => true;
	protected override bool IsBodyPartUsable(BodyPart bodyPart) => bodyPart.Available && IsArm(bodyPart.id) && HasWeapon(bodyPart);
	protected override string StartDialogueText => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string ExecuteDialogueText => $"{actor.name}用{actorBodyPart.Name}刺击{TargetCharacter.name}的{TargetCombatObject.Name}!";
}
