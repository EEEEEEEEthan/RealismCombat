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
	CombatState combatState = null!;
	public CharacterNode AddCharacter(CharacterData data)
	{
		var characterNode = CharacterNode.Create();
		characterNode.CharacterData = data;
		AddChild(characterNode);
		PlaceCharacter(characterNode, data.team);
		return characterNode;
	}
	void PlaceCharacter(CharacterNode node, byte team)
	{
		const float width = 320f;
		const float height = 360f;
		const float margin = 50f;
		const float top = 80f;
		if (team == 0)
		{
			node.AnchorLeft = 0f;
			node.AnchorRight = 0f;
			node.AnchorTop = 0f;
			node.AnchorBottom = 0f;
			node.OffsetLeft = margin;
			node.OffsetTop = top;
			node.OffsetRight = node.OffsetLeft + width;
			node.OffsetBottom = node.OffsetTop + height;
		}
		else
		{
			node.AnchorLeft = 1f;
			node.AnchorRight = 1f;
			node.AnchorTop = 0f;
			node.AnchorBottom = 0f;
			node.OffsetRight = -margin;
			node.OffsetLeft = node.OffsetRight - width;
			node.OffsetTop = top;
			node.OffsetBottom = node.OffsetTop + height;
		}
	}
}
