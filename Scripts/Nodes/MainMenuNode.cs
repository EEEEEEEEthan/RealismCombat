using Godot;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Nodes;
partial class MainMenuNode : Node
{
	public static MainMenuNode Create(MenuState menuState)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.menuScene).Instantiate<MainMenuNode>();
		instance.menuState = menuState;
		return instance;
	}
	[Export] Button buttonNewGame = null!;
	[Export] Button buttonLoadGame = null!;
	MenuState menuState = null!;
	public override void _Ready()
	{
		buttonNewGame.Pressed += OnClickNewGame;
		buttonLoadGame.Pressed += OnClickLoadGame;
	}
	void OnClickNewGame() => menuState.NewGame();
	void OnClickLoadGame() => menuState.LoadGame();
}
