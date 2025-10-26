using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Data;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
class ActionState(CombatState combatState, CombatData combatData) : CombatChildState(combatState: combatState, combatData: combatData)
{
	public override string Name => "进行行动";
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>();
	private protected override void OnExit() { }
	private protected override string GetStatus() => "行动中";
}
