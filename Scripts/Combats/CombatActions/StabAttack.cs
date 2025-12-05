using Godot;
/// <summary>
///     刺击攻击，只允许手臂使用
/// </summary>
public class StabAttack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: AttackBase(actor, actorBodyPart, target, combatTarget, combat)
{
	/// <summary>
	///     验证攻击是否可以使用
	/// </summary>
	public static bool CanUse(BodyPart bodyPart) => IsArm(bodyPart.id);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}刺击{target.name}的{combatTarget.Name}!";
	protected override int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
}
