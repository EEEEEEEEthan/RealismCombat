using Godot;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Nodes;
partial class Game : Node
{
	public static Game Create(GameState gameState)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.gameScene).Instantiate<Game>();
		instance.gameState = gameState;
		return instance;
	}
	GameState gameState = null!;
}
