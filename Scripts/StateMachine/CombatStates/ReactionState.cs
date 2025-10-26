using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class ReactionState : State
{
	readonly CombatState combatState;
	public ReactionState(CombatState combatState) : base(rootNode: combatState.rootNode, owner: combatState) => this.combatState = combatState;
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override void OnExit() { }
	private protected override string GetStatus() => "反应中";
}
