using System.Linq;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes.Combats;
public abstract class Action(Character actor)
{
	public readonly Character actor = actor;
	public abstract Task ExecuteTask();
}
public class Attack(Character actor, Character? target = null) : Action(actor)
{
	Character? selectedTarget = target;
	public void SetTarget(Character target) => selectedTarget = target;
	public override async Task ExecuteTask()
	{
		if (selectedTarget == null) return;
		var dialogue = DialogueManager.CreateGenericDialogue($"{actor.name}发起攻击!");
		await dialogue.PrintDone;
		var damage = (int)(GD.Randi() % 3u) + 1;
		var newHp = selectedTarget.hp.value - damage;
		if (newHp < 0) newHp = 0;
		if (newHp > selectedTarget.hp.maxValue) newHp = selectedTarget.hp.maxValue;
		selectedTarget.hp.value = newHp;
		dialogue.AddText($"{selectedTarget.name}受到了{damage}点伤害，剩余{selectedTarget.hp.value}/{selectedTarget.hp.maxValue}");
		if (!selectedTarget.IsAlive) dialogue.AddText($"{selectedTarget.name}倒下了");
		dialogue.AddText($"{actor.name}消耗了5行动力");
		await dialogue;
		actor.actionPoint.value -= 5;
		if (actor.actionPoint.value < 0) actor.actionPoint.value = 0;
	}
}
public abstract partial class CombatInput : Node
{
	readonly Combat combat;
	protected CombatInput(Combat combat) => this.combat = combat;
	public abstract Task<Action> MakeDecisionTask(Character character);
	protected Character[] GetOpponents(Character character) => combat.Allies.Contains(character) ? combat.Enemies : combat.Allies;
	protected Character[] GetAliveOpponents(Character character) => GetOpponents(character).Where(c => c.IsAlive).ToArray();
	protected Character? GetRandomOpponent(Character character)
	{
		var alive = GetAliveOpponents(character);
		if (alive.Length == 0) return null;
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)alive.Length);
		return alive[index];
	}
}
public partial class PlayerInput : CombatInput
{
	public PlayerInput(Combat combat) : base(combat) { }
	public override async Task<Action> MakeDecisionTask(Character character)
	{
		await DialogueManager.CreateMenuDialogue(
			new MenuOption { title = "攻击", description = "攻击敌人", }
		);
		var attack = new Attack(character);
		var aliveOpponents = GetAliveOpponents(character);
		if (aliveOpponents.Length == 0) return attack;
		if (aliveOpponents.Length == 1)
		{
			attack.SetTarget(aliveOpponents[0]);
			return attack;
		}
		var options = aliveOpponents
			.Select(o => new MenuOption
			{
				title = o.name,
				description = $"生命 {o.hp.value}/{o.hp.maxValue}",
			})
			.ToArray();
		var menu = DialogueManager.CreateMenuDialogue(options);
		var selected = await menu;
		attack.SetTarget(aliveOpponents[selected]);
		return attack;
	}
}
public partial class AIInput : CombatInput
{
	public AIInput(Combat combat) : base(combat) { }
	public override Task<Action> MakeDecisionTask(Character character)
	{
		var attack = new Attack(character);
		var target = GetRandomOpponent(character);
		if (target != null) attack.SetTarget(target);
		return Task.FromResult<Action>(attack);
	}
}
