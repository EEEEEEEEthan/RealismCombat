using RealismCombat.Data;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.CombatStates;
abstract class CombatChildState(CombatState combatState, CombatData combatData) : State(rootNode: combatState.rootNode, owner: combatState)
{
	public readonly CombatState combatState = combatState;
	public readonly CombatData combatData = combatData;
}
