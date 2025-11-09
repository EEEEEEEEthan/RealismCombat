using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
namespace RealismCombat.Combats;
public abstract class CombatAction(Combat combat, Character actor, double ap1, double ap2)
{
	public readonly Combat combat = combat;
	public readonly double startTime = combat.Time;
	protected readonly Character actor = actor;
	double ElapsedTime => combat.Time - startTime;
	double ElapsedActionPoints => ElapsedTime * actor.speed.value;
	public async Task StartTask()
	{
		actor.actionPoint.value -= ap1;
		await OnStartTask();
	}
	public async Task<bool> UpdateTask()
	{
		if (actor.actionPoint.value >= actor.actionPoint.maxValue)
		{
			actor.actionPoint.value -= ap2;
			await OnExecute();
			return false;
		}
		return true;
	}
	protected abstract Task OnStartTask();
	protected abstract Task OnExecute();
}
public class Attack(Combat combat, Character actor, Character? target = null) : CombatAction(combat, actor, 3, 3)
{
	Character? selectedTarget = target;
	public void SetTarget(Character target) => selectedTarget = target;
	protected override async Task OnStartTask()
	{
		if (selectedTarget == null) return;
		await DialogueManager.CreateGenericDialogue($"{actor.name}抬起长剑!");
	}
	protected override async Task OnExecute()
	{
		var damage = (int)(GD.Randi() % 3u) + 1;
		var newHp = selectedTarget.hp.value - damage;
		if (newHp < 0) newHp = 0;
		if (newHp > selectedTarget.hp.maxValue) newHp = selectedTarget.hp.maxValue;
		selectedTarget.hp.value = newHp;
		var dialogue = DialogueManager.CreateGenericDialogue($"{actor.name}挥剑斩向{selectedTarget.name}!");
		await dialogue.PrintDone;
		dialogue.AddText($"{selectedTarget.name}受到了{damage}点伤害，剩余{selectedTarget.hp.value}/{selectedTarget.hp.maxValue}");
		if (!selectedTarget.IsAlive) dialogue.AddText($"{selectedTarget.name}倒下了");
		await dialogue;
		actor.actionPoint.value -= 5;
		if (actor.actionPoint.value < 0) actor.actionPoint.value = 0;
	}
}
