using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes.Combats;
public interface IAction
{
	Task ExecuteTask();
}
public class Attack : IAction
{
	public Task ExecuteTask()
	{
		Log.Print("执行攻击动作");
		return Task.CompletedTask;
	}
}
public abstract partial class CombatInput : Node
{
	public abstract Task<IAction> MakeDecisionTask();
}
public partial class PlayerInput : CombatInput
{
	public override async Task<IAction> MakeDecisionTask()
	{
		var menu = DialogueManager.CreateMenuDialogue(
			new MenuOption { title = "攻击", description = "攻击敌人", }
		);
		var choice = await menu;
		return new Attack();
	}
}
public partial class AIInput : CombatInput
{
	public override Task<IAction> MakeDecisionTask() => Task.FromResult<IAction>(new Attack());
}
