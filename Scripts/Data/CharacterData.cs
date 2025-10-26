using RealismCombat.Nodes;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
class CharacterData(CombatNode combatNode, string name, byte team)
{
	/// <summary>
	///     所属战斗引用
	/// </summary>
	public readonly CombatNode combatNode = combatNode;
	/// <summary>
	///     角色名字
	/// </summary>
	public readonly string name = name;
	/// <summary>
	///     角色队伍
	/// </summary>
	public readonly byte team = team;
}
