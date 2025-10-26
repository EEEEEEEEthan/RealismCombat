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
	Button buttonNewGame = null!;
	Button buttonLoadGame = null!;
	MenuState menuState = null!;
	public override void _Ready()
	{
		buttonNewGame = GetNode<Button>("ButtonNewGame");
		buttonNewGame.Pressed += OnClickNewGame;
		buttonLoadGame = GetNode<Button>("ButtonLoadGame");
		buttonLoadGame.Pressed += OnClickLoadGame;
	}
	void OnClickNewGame() => menuState.NewGame();
	void OnClickLoadGame() => menuState.LoadGame();
}
