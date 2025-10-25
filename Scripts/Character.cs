namespace RealismCombat;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
public class Character
{
	/// <summary>
	///     所属战斗引用
	/// </summary>
	public readonly Combat combat;

	/// <summary>
	///     角色名字
	/// </summary>
	public readonly string name;

	/// <summary>
	///     角色队伍
	/// </summary>
	public readonly byte team;

	public Character(Combat combat, string name, byte team)
	{
		this.combat = combat;
		this.name = name;
		this.team = team;
	}
}

