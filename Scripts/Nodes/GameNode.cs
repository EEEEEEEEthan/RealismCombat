using Godot;
using RealismCombat;
using RealismCombat.Data;
public partial class GameNode : Node
{
	GameData gameData = null!;

	public static GameNode FromLoad(GameData gameData)
	{
		var gameNode = Create();
		gameNode.gameData = gameData;
		return gameNode;
	}
	public static GameNode FromNew()
	{
		var gameNode = Create();
		gameNode.gameData = new();
		return gameNode;
	}
	static GameNode Create()
	{
		return GD.Load<PackedScene>(ResourceTable.game).Instantiate<GameNode>();
	}
}
