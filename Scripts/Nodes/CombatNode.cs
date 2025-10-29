using System;
using System.Linq;
using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
	public abstract class State(CombatNode combatNode)
	{
		public static State Create(CombatNode combatNode)
		{
			var combatData = combatNode.combatData;
			if (combatData.characters.Count == 0) throw new InvalidOperationException("战斗数据中没有角色");
			return combatData.state switch
			{
				RoundInProgressState.serializeId => new RoundInProgressState(combatNode),
				CharacterTurnState.serializeId => new CharacterTurnState(combatNode, combatData.characters[combatData.currentCharacterIndex]),
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
		public readonly CombatNode combatNode = combatNode;
		public abstract void Update(double deltaTime);
	}
	public class RoundInProgressState : State
	{
		public const byte serializeId = 0;
		public RoundInProgressState(CombatNode combatNode) : base(combatNode)
		{
			combatNode.CurrentState = this;
		}
		public override void Update(double deltaTime)
		{
		}
	}
	public class CharacterTurnState : State
	{
		public const byte serializeId = 1;
		public readonly CharacterData character;
		public CharacterTurnState(CombatNode combatNode, CharacterData character) : base(combatNode)
		{
			this.character = character;
			var characterIndex = (byte)combatNode.combatData.characters.IndexOf(character);
			combatNode.combatData.currentCharacterIndex = characterIndex;
			combatNode.CurrentState = this;
			Log.Print($"现在是 {character.name} 的回合");
		}
		public override void Update(double deltaTime)
		{
		}
	}
	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		combatNode.CurrentState = State.Create(combatNode: combatNode);
		return combatNode;
	}
	State state = null!;
	CombatData combatData = null!;
	GameNode gameNode = null!;
	[Export] Container characterContainer = null!;
	State CurrentState
	{
		get => state;
		set
		{
			state = value;
			combatData.state = value switch
			{
				RoundInProgressState => RoundInProgressState.serializeId,
				CharacterTurnState => CharacterTurnState.serializeId,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}
	public override void _Ready()
	{
		foreach (var character in combatData.characters)
		{
			var characterNode = CharacterNode.Create();
			characterNode.CharacterData = character;
			characterContainer.AddChild(characterNode);
		}
	}
	public override void _Process(double delta)
	{
		CurrentState.Update(delta);
	}
}
