using Godot;
using RealismCombat.Commands.GameCommands;
namespace RealismCombat.Nodes;
partial class PrepareNode : Node
{
	public static PrepareNode Create(ProgramRootNode programRootNode)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.battlePrepareScene).Instantiate<PrepareNode>();
		instance.programRootNode = programRootNode;
		return instance;
	}
	ProgramRootNode programRootNode = null!;
	BaseButton buttonNextCombat = null!;
	public override void _Ready()
	{
		buttonNextCombat = GetNode<Button>("ButtonNextCombat");
		buttonNextCombat.Pressed += OnPressed;
	}
	void OnPressed() => _ = programRootNode.State.ExecuteCommandTask(StartCombatCommand.name);
}
