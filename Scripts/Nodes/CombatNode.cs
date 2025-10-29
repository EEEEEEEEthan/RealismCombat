using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		return combatNode;
	}
	CombatData combatData = null!;
	ProgramRootNode root = null!;
	GameNode gameNode = null!;
	public override void _Ready() => root = GetParent().GetParent<ProgramRootNode>();
}
