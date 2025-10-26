using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.StateMachine.GameStates;
class CombatState : State
{
	readonly GameState gameState;
	readonly Combat combat;
	public CombatState(GameState gameState) : base(root: gameState.root, owner: gameState)
	{
		this.gameState = gameState;
		combat = Combat.Create(this);
		gameState.game.AddChild(combat);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override void OnExit() => combat.QueueFree();
	private protected override string GetStatus() => "战斗中";
}
