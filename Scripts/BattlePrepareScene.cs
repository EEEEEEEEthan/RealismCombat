using Godot;
using RealismCombat.Commands;
namespace RealismCombat;
public partial class BattlePrepareScene : Node
{
	public static BattlePrepareScene Create(GameRoot gameRoot)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.battlePrepareScene).Instantiate<BattlePrepareScene>();
		instance.gameRoot = gameRoot;
		return instance;
	}
	GameRoot gameRoot = null!;
	BaseButton buttonNextCombat = null!;
	public override void _Ready()
	{
		buttonNextCombat = GetChild<Button>(0);
		buttonNextCombat.Pressed += OnPressed;
	}
	void OnPressed() => gameRoot.State.ExecuteCommand(StartCombatCommand.name);
}
