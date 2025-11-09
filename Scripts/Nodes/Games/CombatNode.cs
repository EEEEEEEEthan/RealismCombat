using Godot;
using RealismCombat.Characters;
namespace RealismCombat.Nodes.Games;
public partial class CombatNode : Node
{
	static CharacterNode CreateCharacterNode()
	{
		PackedScene scene = ResourceTable.characterNodeScene;
		return scene.Instantiate<CharacterNode>();
	}
	VBoxContainer? playerTeamContainer;
	VBoxContainer? enemyTeamContainer;
	VBoxContainer PlayerTeamContainer => playerTeamContainer ??= GetNode<VBoxContainer>("SafeArea/PlayerTeamContainer");
	VBoxContainer EnemyTeamContainer => enemyTeamContainer ??= GetNode<VBoxContainer>("SafeArea/EnemyTeamContainer");
	public void Initialize(Character[] allies, Character[] enemies)
	{
		foreach (var child in PlayerTeamContainer.GetChildren()) child.QueueFree();
		foreach (var child in EnemyTeamContainer.GetChildren()) child.QueueFree();
		foreach (var _ in allies) PlayerTeamContainer.AddChild(CreateCharacterNode());
		foreach (var _ in enemies) EnemyTeamContainer.AddChild(CreateCharacterNode());
	}
}
