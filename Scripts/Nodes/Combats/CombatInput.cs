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
public class Attack(Character actor) : Action(actor)
{
	public override async Task ExecuteTask()
	{
		var dialogue = DialogueManager.CreateGenericDialogue($"{actor.name}发起攻击!");
		await dialogue.PrintDone;
		dialogue.AddText($"{actor.name}消耗了5行动力");
		await dialogue;
		actor.actionPoint.value -= 5;
	}
}
public abstract partial class CombatInput : Node
{
	public abstract Task<Action> MakeDecisionTask(Character character);
}
public partial class PlayerInput : CombatInput
{
	public override async Task<Action> MakeDecisionTask(Character character)
	{
		await DialogueManager.CreateMenuDialogue(
			new MenuOption { title = "攻击", description = "攻击敌人", }
		);
		return new Attack(character);
	}
}
public partial class AIInput : CombatInput
{
	public override Task<Action> MakeDecisionTask(Character character) => Task.FromResult<Action>(new Attack(character));
}
