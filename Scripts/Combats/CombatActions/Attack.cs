using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
namespace RealismCombat.Combats.CombatActions;
public class Attack(Character actor, BodyPart actorBodyPart, Character target, ICombatTarget combatTarget, Combat combat) : CombatAction(actor, combat, 3, 3)
{
	static int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
	public Character Actor => actor;
	public BodyPart ActorBodyPart => actorBodyPart;
	public Character Target => target;
	public ICombatTarget CombatTarget => combatTarget;
	protected override async Task OnStartTask() => await DialogueManager.CreateGenericDialogue($"{actor.name}抬起{actorBodyPart.Name}开始蓄力...");
	protected override async Task OnExecute()
	{
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetPKPosition(actor);
		var targetPosition = combat.combatNode.GetPKPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		var startDialogue = DialogueManager.CreateGenericDialogue($"{actor.name}用{actorBodyPart.Name}攻击{target.name}的{combatTarget.Name}!");
		await startDialogue;
		var reaction = await combat.HandleIncomingAttack(this);
		var finalTarget = combatTarget;
		var attackHit = true;
		var resultMessages = new List<string>();
		var hitPosition = combat.combatNode.GetHitPosition(actor);
		actorNode.MoveTo(hitPosition);
		switch (reaction.Type)
		{
			case ReactionType.Dodge:
				await Task.Delay(10);
				targetNode.MoveTo(combat.combatNode.GetDogePosition(target));
				resultMessages.Add($"{target.name}及时闪避, 攻击落空");
				attackHit = false;
				break;
			case ReactionType.Block:
				await Task.Delay(50);
				targetNode.MoveTo(targetPosition + Vector2.Up * 12);
				await Task.Delay(100);
				targetNode.MoveTo(targetPosition);
				finalTarget = reaction.BlockTarget!;
				AudioManager.PlaySfx(ResourceTable.blockSound);
				resultMessages.Add($"{target.name}使用{finalTarget.Name}进行了格挡");
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
				break;
			case ReactionType.None:
				await Task.Delay(100);
				targetNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		actorNode.MoveTo(actorPosition);
		if (attackHit)
		{
			var damage = CalculateDamage();
			finalTarget.HitPoint.value = Mathf.Clamp(finalTarget.HitPoint.value - damage, 0, finalTarget.HitPoint.maxValue);
			targetNode.FlashPropertyNode(finalTarget);
			resultMessages.Add($"{target.name}的{finalTarget.Name}受到了{damage}点伤害，剩余{finalTarget.HitPoint.value}/{finalTarget.HitPoint.maxValue}");
			if (!finalTarget.Available)
				resultMessages.Add(finalTarget is BodyPart ? $"{target.name}的{finalTarget.Name}失去战斗能力" : $"{target.name}的{finalTarget.Name}已无法继续使用");
			if (!target.IsAlive) resultMessages.Add($"{target.name}倒下了");
		}
		else if (resultMessages.Count == 0)
		{
			resultMessages.Add($"{target.name}成功避开了攻击");
		}
		var resultDialogue = DialogueManager.CreateGenericDialogue(resultMessages.ToArray());
		await resultDialogue;
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - 5);
	}
}
