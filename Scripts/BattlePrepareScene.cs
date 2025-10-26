using Godot;
using RealismCombat.Commands.GameCommands;
using RealismCombat.Nodes;
namespace RealismCombat;
partial class BattlePrepareScene : Node
{
	public static BattlePrepareScene Create(ProgramRoot programRoot)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.battlePrepareScene).Instantiate<BattlePrepareScene>();
		instance.programRoot = programRoot;
		return instance;
	}
	ProgramRoot programRoot = null!;
	BaseButton buttonNextCombat = null!;
	public override void _Ready()
	{
		buttonNextCombat = GetNode<Button>("ButtonNextCombat");
		buttonNextCombat.Pressed += OnPressed;
	}
	void OnPressed() => programRoot.State.ExecuteCommand(StartCombatCommand.name);
}
