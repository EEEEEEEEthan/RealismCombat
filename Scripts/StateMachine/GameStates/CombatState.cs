using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.StateMachine.GameStates;
class CombatState : State
{
	readonly GameState gameState;
	readonly CombatData combatData;
	readonly CombatNode combatNode;
	public CombatState(GameState gameState) : base(rootNode: gameState.rootNode, owner: gameState)
	{
		this.gameState = gameState;
		combatData = new();
		combatNode = CombatNode.Create(this);
		gameState.gameNode.AddChild(combatNode);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override void OnExit() => combatNode.QueueFree();
	private protected override string GetStatus() => "战斗中";
}
