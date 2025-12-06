using Godot;
/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: AttackBase(actor, actorBodyPart, target, combatTarget, combat)
{
	internal override double DodgeImpact => 0.4;
	internal override double BlockImpact => 0.45;
	/// <summary>
	///     检查身体部位是否适配此攻击类型
	/// </summary>
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => bodyPart.id == BodyPartCode.Torso;
	/// <summary>
	///     验证攻击是否可以使用（综合验证，包括身体部位适配性和可用性）
	/// </summary>
	public static bool CanUse(BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}撞击{target.name}的{combatTarget.Name}!";
	protected override int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
}
