using System.Collections.Generic;
using Godot;
using RealismCombat.Characters;
using RealismCombat.Combats;
namespace RealismCombat.Nodes.Games;
public partial class CombatNode : Node
{
	static CharacterNode CreateCharacterNode()
	{
		PackedScene scene = ResourceTable.characterNodeScene;
		return scene.Instantiate<CharacterNode>();
	}
	readonly Dictionary<Character, CharacterNode> characterNodes = new();
	VBoxContainer? playerTeamContainer;
	VBoxContainer? enemyTeamContainer;
	VBoxContainer PlayerTeamContainer => playerTeamContainer ??= GetNode<VBoxContainer>("SafeArea/PlayerTeamContainer");
	VBoxContainer EnemyTeamContainer => enemyTeamContainer ??= GetNode<VBoxContainer>("SafeArea/EnemyTeamContainer");
	public void Initialize(Combat combat)
	{
		foreach (var child in PlayerTeamContainer.GetChildren()) child.QueueFree();
		foreach (var child in EnemyTeamContainer.GetChildren()) child.QueueFree();
		characterNodes.Clear();
		foreach (var character in combat.Allies)
		{
			var node = CreateCharacterNode();
			node.IsEnemyTheme = false;
			node.Initialize(combat, character);
			PlayerTeamContainer.AddChild(node);
			characterNodes[character] = node;
		}
		foreach (var character in combat.Enemies)
		{
			var node = CreateCharacterNode();
			node.IsEnemyTheme = true;
			node.Initialize(combat, character);
			EnemyTeamContainer.AddChild(node);
			characterNodes[character] = node;
		}
	}
	public CharacterNode? TryGetCharacterNode(Character character)
	{
		if (characterNodes.TryGetValue(character, out var node)) return node;
		return null;
	}
}
