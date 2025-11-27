using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Characters;
namespace RealismCombat.Combats.CombatActions;
/// <summary>
///     抓取攻击，只允许没有武器的手臂使用
/// </summary>
public class GrabAttack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat)
	: AttackBase(actor, actorBodyPart, target, combatTarget, combat)
{
	/// <summary>
	///     验证攻击是否可以使用
	/// </summary>
	public static bool CanUse(BodyPart bodyPart) => IsArm(bodyPart.id) && !HasWeapon(bodyPart);
	protected override string GetStartDialogueText() => $"{actor.name}抬起{actorBodyPart.Name}开始蓄力...";
	protected override string GetExecuteDialogueText() => $"{actor.name}用{actorBodyPart.Name}抓取{target.name}的{combatTarget.Name}!";
	protected override int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
	/// <summary>
	///     抓取攻击命中后有概率施加束缚和擒拿buff
	/// </summary>
	protected override async Task OnAttackHit(ICombatTarget finalTarget, List<string> resultMessages)
	{
		var grabSuccessChance = 0.5f;
		if (GD.Randf() < grabSuccessChance)
		{
			if (target.torso is IBuffOwner targetTorsoBuffOwner)
			{
				var restrainedBuff = new Buff(BuffCode.Restrained, actor);
				targetTorsoBuffOwner.AddBuff(restrainedBuff);
				resultMessages.Add($"{target.name}的{finalTarget.Name}被{actor.name}抓住了!");
			}
			if (actorBodyPart is IBuffOwner actorBuffOwner)
			{
				var grapplingBuff = new Buff(BuffCode.Grappling, actor);
				actorBuffOwner.AddBuff(grapplingBuff);
				resultMessages.Add($"{actor.name}的{actorBodyPart.Name}正在擒拿{target.name}!");
			}
		}
		await Task.CompletedTask;
	}
}
