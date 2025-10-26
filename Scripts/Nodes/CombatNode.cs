using Godot;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.Nodes;
/// <summary>
///     战斗核心对象（占位），仅负责承载战斗期的上下文。
/// </summary>
partial class CombatNode : Node
{
	public static CombatNode Create(CombatState combatState)
	{
		var instance = GD.Load<PackedScene>(ResourceTable.combatScene).Instantiate<CombatNode>();
		instance.combatState = combatState;
		return instance;
	}
	CombatState combatState = null!;
}
