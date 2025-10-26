using Godot;
using RealismCombat.Commands;
namespace RealismCombat;
public partial class BattlePrepareScene : Node
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
		buttonNextCombat = GetChild<Button>(0);
		buttonNextCombat.Pressed += OnPressed;
	}
	void OnPressed() => programRoot.State.ExecuteCommand(StartCombatCommand.name);
}
