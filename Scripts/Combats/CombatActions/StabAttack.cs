using Godot;
/// <summary>
///     刺击攻击，只允许手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: AttackBase(actor, actorBodyPart, target, combatTarget, combat)
{
	internal override double DodgeImpact => 0.55;
	internal override double BlockImpact => 0.5;
	internal override bool UsesWeapon => true;
	/// <summary>
	///     检查身体部位是否适配此攻击类型
	/// </summary>
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => IsArm(bodyPart.id);
	/// <summary>
	///     验证攻击是否可以使用（综合验证，包括身体部位适配性和可用性）
	/// </summary>
	public static bool CanUse(BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}刺击{target.name}的{combatTarget.Name}!";
	protected override int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
}
