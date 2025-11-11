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
	Control? playerPkPosition;
	Control? enemyPkPosition;
	Control? playerReadyPosition;
	Control? enemyReadyPosition;
	public Control PlayerPkPosition => playerPkPosition ??= GetNode<Control>("SafeArea/PKContainer/PlayerPosition");
	public Control EnemyPkPosition => enemyPkPosition ??= GetNode<Control>("SafeArea/PKContainer/EnemyPosition");
	public Control PlayerReadyPosition => playerReadyPosition ??= GetNode<Control>("SafeArea/ReadyContainer/PlayerPosition");
	public Control EnemyReadyPosition => enemyReadyPosition ??= GetNode<Control>("SafeArea/ReadyContainer/EnemyPosition");
	public Combat Combat { get; private set; } = null!;
	VBoxContainer PlayerTeamContainer => playerTeamContainer ??= GetNode<VBoxContainer>("SafeArea/PlayerTeamContainer");
	VBoxContainer EnemyTeamContainer => enemyTeamContainer ??= GetNode<VBoxContainer>("SafeArea/EnemyTeamContainer");
	public override void _Ready()
	{
		base._Ready();
		PlayerPkPosition.Modulate = EnemyPkPosition.Modulate = GameColors.transparent;
		PlayerReadyPosition.Modulate = EnemyReadyPosition.Modulate = GameColors.transparent;
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
			PlayerTeamContainer.AddChild(node);
			node.Initialize(combat, character);
			characterNodes[character] = node;
		}
		foreach (var character in combat.Enemies)
		{
			var node = CreateCharacterNode();
			node.IsEnemyTheme = true;
			EnemyTeamContainer.AddChild(node);
			node.Initialize(combat, character);
			characterNodes[character] = node;
		}
	}
	public CharacterNode GetCharacterNode(Character character) => characterNodes[character];
	public Vector2 GetPKPosition(Character character)
	{
		if (Combat.Allies.Contains(character)) return PlayerPkPosition.GlobalPosition;
		return EnemyPkPosition.GlobalPosition;
	}
	public Vector2 GetReadyPosition(Character character)
	{
		if (Combat.Allies.Contains(character)) return PlayerReadyPosition.GlobalPosition;
		return EnemyReadyPosition.GlobalPosition;
	}
}
