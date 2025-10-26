using System.Collections.Generic;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.Commands.CombatCommands;
abstract class CombatCommand : Command
{
	public readonly CombatState combatState;
	protected CombatCommand(CombatState combatState, IReadOnlyDictionary<string, string>? arguments = null) :
		base(rootNode: combatState.rootNode, arguments: arguments) =>
		this.combatState = combatState;
	protected CombatCommand(CombatState combatState, string command) : base(rootNode: combatState.rootNode, command: command) => this.combatState = combatState;
}
