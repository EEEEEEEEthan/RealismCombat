using Godot;
using RealismCombat.Commands.ProgramCommands;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Nodes;
partial class MainMenu : Node
{
	public static MainMenu Create(MenuState menuState)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.menuScene).Instantiate<MainMenu>();
		instance.menuState = menuState;
		return instance;
	}
	Button buttonNewGame = null!;
	MenuState menuState = null!;
	public override void _Ready()
	{
		buttonNewGame = GetNode<Button>("Button");
		buttonNewGame.Pressed += onClick;
	}
	void onClick() => menuState.ExecuteCommand(StartNewGameCommand.name);
}
