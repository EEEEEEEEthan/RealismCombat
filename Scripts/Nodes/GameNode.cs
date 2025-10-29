using Godot;
using RealismCombat;
using RealismCombat.Data;
public partial class GameNode : Node
{
	GameData gameData = null!;

	public static GameNode FromLoad(GameData gameData)
	{
		
	}
	public static GameNode FromNew()
	{
		
	}
	static GameNode Create()
	{
		return GD.Load<PackedScene>(ResourceTable.game).Instantiate<GameNode>();
	}
}
