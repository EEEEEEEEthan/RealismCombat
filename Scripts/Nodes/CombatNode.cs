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
			var currentCharacter = combatData.characters.OrderByDescending(c => c.actionPoint).First();
			return new CharacterTurnState(combatNode: combatNode, character: currentCharacter);
		}
		public readonly CombatNode combatNode = combatNode;
	}
	public class RoundInProgressState : State
	{
		public RoundInProgressState(CombatNode combatNode) : base(combatNode)
		{
			combatNode.state = this;
		}
	}
	public class CharacterTurnState : State
	{
		public readonly CharacterData character;
		public CharacterTurnState(CombatNode combatNode, CharacterData character) : base(combatNode)
		{
			combatNode.state = this;
			this.character = character;
			Log.Print($"现在是 {character.name} 的回合");
		}
	}
	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		combatNode.state = State.Create(combatNode: combatNode);
		return combatNode;
	}
	State state = null!;
	CombatData combatData = null!;
	GameNode gameNode = null!;
}
