using System;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
namespace RealismCombat.Combats;
public abstract class CombatAction(Character actor, Combat combat, double preCastActionPointCost, double postCastActionPointCost)
{
	protected readonly Character actor = actor;
	protected readonly Combat combat = combat;
	public async Task StartTask()
	{
		actor.actionPoint.value -= preCastActionPointCost;
		await OnStartTask();
	}
	public async Task<bool> UpdateTask()
	{
		if (actor.actionPoint.value >= actor.actionPoint.maxValue)
		{
			actor.actionPoint.value -= postCastActionPointCost;
			await OnExecute();
			return false;
		}
		return true;
	}
	protected abstract Task OnStartTask();
	protected abstract Task OnExecute();
}
public class Attack(Character actor, Character target, ICombatTarget combatTarget, Combat combat) : CombatAction(actor, combat, 3, 3)
{
	static int CalculateDamage() => (int)(GD.Randi() % 3u) + 1;
	protected override async Task OnStartTask() => await DialogueManager.CreateGenericDialogue($"{actor.name}抬起长剑开始蓄力...");
	protected override async Task OnExecute()
	{
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetCharacterPosition(actor);
		var targetPosition = combat.combatNode.GetCharacterPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		var damage = CalculateDamage();
		var dialogue = DialogueManager.CreateGenericDialogue($"{actor.name}挥剑斩向{target.name}的{combatTarget.TargetName}!");
		await dialogue.PrintDone;
		targetNode.Shake();
		AudioManager.PlaySfx(ResourceTable.retroHurt1);
		combatTarget.HitPoint.value = Mathf.Clamp(combatTarget.HitPoint.value - damage, 0, combatTarget.HitPoint.maxValue);
		dialogue.AddText($"{target.name}的{combatTarget.TargetName}受到了{damage}点伤害，剩余{combatTarget.HitPoint.value}/{combatTarget.HitPoint.maxValue}");
		if (!combatTarget.Available) dialogue.AddText($"{target.name}的{combatTarget.TargetName}失去战斗能力");
		if (!target.IsAlive) dialogue.AddText($"{target.name}倒下了");
		await dialogue;
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - 5);
	}
}
