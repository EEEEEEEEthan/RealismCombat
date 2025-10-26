using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class TurnProgressState : State
{
	readonly CombatState combatState;
	public override string Name => "回合进行中";
	public TurnProgressState(CombatState combatState) : base(rootNode: combatState.rootNode, owner: combatState) => this.combatState = combatState;
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override void OnExit() { }
	private protected override string GetStatus() => "回合进行中";
}
