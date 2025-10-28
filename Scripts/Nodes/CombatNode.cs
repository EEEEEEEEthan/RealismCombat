using Godot;
using RealismCombat.Data;
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
	[Export] Container characterContainer = null!;
	CombatState combatState = null!;
	public CharacterNode AddCharacter(CharacterData data)
	{
		var characterNode = CharacterNode.Create();
		characterNode.CharacterData = data;
		characterContainer.AddChild(characterNode);
		PlaceCharacter(node: characterNode, team: data.team);
		return characterNode;
	}
	void PlaceCharacter(CharacterNode node, byte team) =>
		node.SetAnchorsAndOffsetsPreset(team == 0 ? Control.LayoutPreset.CenterLeft : Control.LayoutPreset.CenterRight);
}
