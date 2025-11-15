using Godot;
using RealismCombat.Characters;
namespace RealismCombat.Combats.CombatActions;
/// <summary>
///     撞击攻击，只允许躯干使用
/// </summary>
public class ChargeAttack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: AttackBase(actor, actorBodyPart, target, combatTarget, combat)
{
	/// <summary>
	///     验证攻击是否可以使用
	/// </summary>
	public static bool CanUse(BodyPart bodyPart) => bodyPart.id == BodyPartCode.Torso;
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}撞击{target.name}的{combatTarget.Name}!";
	protected override int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
}
