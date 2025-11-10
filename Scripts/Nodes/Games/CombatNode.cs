using System.Collections.Generic;
using System.Linq;
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
	Control? playerPosition;
	Control? enemyPosition;
	public Control PlayerPosition => playerPosition ??= GetNode<Control>("SafeArea/PKContainer/PlayerPosition");
	public Control EnemyPosition => enemyPosition ??= GetNode<Control>("SafeArea/PKContainer/EnemyPosition");
	public Combat Combat { get; private set; }
	VBoxContainer PlayerTeamContainer => playerTeamContainer ??= GetNode<VBoxContainer>("SafeArea/PlayerTeamContainer");
	VBoxContainer EnemyTeamContainer => enemyTeamContainer ??= GetNode<VBoxContainer>("SafeArea/EnemyTeamContainer");
	public override void _Ready()
	{
		base._Ready();
		PlayerPosition.Modulate = EnemyPosition.Modulate = Colors.Transparent;
	}
	public void Initialize(Combat combat)
	{
		Combat = combat;
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
	public CharacterNode GetCharacterNode(Character character) => characterNodes[character];
	public Vector2 GetCharacterPosition(Character character)
	{
		if (Combat.Allies.Contains(character)) return PlayerPosition.GlobalPosition;
		return EnemyPosition.GlobalPosition;
	}
}
