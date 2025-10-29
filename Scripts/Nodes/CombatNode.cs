using Godot;
using RealismCombat;
using RealismCombat.Data;

namespace RealismCombat.Nodes;

public partial class CombatNode : Node
{
	CombatData combatData = null!;
	ProgramRootNode root = null!;
	GameNode gameNode = null!;

	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		return combatNode;
	}

	public override void _Ready()
	{
		root = GetParent().GetParent<ProgramRootNode>();
	}
}

