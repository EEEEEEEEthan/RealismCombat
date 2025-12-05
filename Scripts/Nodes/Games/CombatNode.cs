using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
public partial class CombatNode : Node
{
	static CharacterNode CreateCharacterNode()
	{
		PackedScene scene = ResourceTable.characterNodeScene;
		return scene.Instantiate<CharacterNode>();
	}
	readonly Dictionary<Character, CharacterNode> characterNodes = new();
	[field: AllowNull, MaybeNull,] public Control PlayerPkPosition => field ??= GetNode<Control>("SafeArea/PKContainer/PlayerPosition");
	[field: AllowNull, MaybeNull,] public Control EnemyPkPosition => field ??= GetNode<Control>("SafeArea/PKContainer/EnemyPosition");
	[field: AllowNull, MaybeNull,] public Control PlayerReadyPosition => field ??= GetNode<Control>("SafeArea/ReadyContainer/PlayerPosition");
	[field: AllowNull, MaybeNull,] public Control EnemyReadyPosition => field ??= GetNode<Control>("SafeArea/ReadyContainer/EnemyPosition");
	public Combat Combat { get; private set; } = null!;
	[field: AllowNull, MaybeNull,] VBoxContainer PlayerTeamContainer => field ??= GetNode<VBoxContainer>("SafeArea/PlayerTeamContainer");
	[field: AllowNull, MaybeNull,] VBoxContainer EnemyTeamContainer => field ??= GetNode<VBoxContainer>("SafeArea/EnemyTeamContainer");
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
	public Vector2 GetHitPosition(Character character)
	{
		var pos = GetPKPosition(character);
		return Combat.Allies.Contains(character)
			? pos + new Vector2(12, 0)
			: pos + new Vector2(-12, 0);
	}
	public Vector2 GetDogePosition(Character character)
	{
		var pos = GetPKPosition(character);
		return Combat.Allies.Contains(character)
			? pos + new Vector2(-12, 0)
			: pos + new Vector2(12, 0);
	}
}
