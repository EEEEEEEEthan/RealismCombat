using System.Collections.Generic;
namespace RealismCombat;
/// <summary>
///     战斗核心对象（占位），仅负责承载战斗期的上下文。
/// </summary>
public class Combat
{
	/// <summary>
	///     游戏根节点引用
	/// </summary>
	public readonly GameRoot gameRoot;

	/// <summary>
	///     战斗中的所有角色
	/// </summary>
	public readonly List<Character> characters = new();

	public Combat(GameRoot gameRoot)
	{
		this.gameRoot = gameRoot;
	}

	/// <summary>
	///     添加角色到战斗
	/// </summary>
	public void AddCharacter(Character character)
	{
		characters.Add(character);
		Log.Print($"{character.name} 加入战斗");
	}
}


