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
	protected override async Task OnStartTask() => await DialogueManager.CreateGenericDialogue($"{actor.name}抬起长剑!");
	protected override async Task OnExecute()
	{
		var damage = CalculateDamage();
		combatTarget.HitPoint.value = Mathf.Clamp(combatTarget.HitPoint.value - damage, 0, combatTarget.HitPoint.maxValue);
		target.hp.value = Mathf.Clamp(target.hp.value - damage, 0, target.hp.maxValue);
		var node = combat.TryGetCharacterNode(target);
		var dialogue = DialogueManager.CreateGenericDialogue($"{actor.name}挥剑斩向{target.name}的{combatTarget.TargetName}!");
		await dialogue.PrintDone;
		node?.Shake();
		AudioManager.PlaySfx(ResourceTable.retroHurt1);
		dialogue.AddText($"{target.name}的{combatTarget.TargetName}受到了{damage}点伤害，剩余{combatTarget.HitPoint.value}/{combatTarget.HitPoint.maxValue}");
		if (!combatTarget.IsTargetAlive) dialogue.AddText($"{target.name}的{combatTarget.TargetName}失去战斗能力");
		if (!target.IsAlive) dialogue.AddText($"{target.name}倒下了");
		await dialogue;
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - 5);
	}
}
