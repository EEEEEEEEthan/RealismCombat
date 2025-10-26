using Godot;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Nodes;
partial class GameNode : Node
{
	public static GameNode Create(GameState gameState)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.gameScene).Instantiate<GameNode>();
		instance.gameState = gameState;
		return instance;
	}
	GameState gameState = null!;
	Button buttonQuit = null!;
	public override void _Ready()
	{
		buttonQuit = GetNode<Button>("ButtonQuit");
		buttonQuit.Pressed += OnClickQuit;
	}
	void OnClickQuit() => gameState.Quit();
}
