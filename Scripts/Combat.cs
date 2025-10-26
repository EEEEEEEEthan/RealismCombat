using System.Collections.Generic;
using RealismCombat.StateMachine;
namespace RealismCombat;
/// <summary>
///     战斗核心对象（占位），仅负责承载战斗期的上下文。
/// </summary>
class Combat : IStateOwner
{
	/// <summary>
	///     游戏根节点引用
	/// </summary>
	public readonly ProgramRoot programRoot;
	/// <summary>
	///     战斗中的所有角色
	/// </summary>
	public readonly List<Character> characters = [];
	State state;
	/// <summary>
	///     当前战斗状态
	/// </summary>
	public State State
	{
		get => state;
		set
		{
			state = value;
			Log.Print($"战斗进入状态:{state}");
		}
	}
	public Combat(ProgramRoot programRoot) => this.programRoot = programRoot;
	public void Update(double dt) => State.Update(dt);
	/// <summary>
	///     添加角色到战斗
	/// </summary>
	public void AddCharacter(Character character)
	{
		characters.Add(character);
		Log.Print($"{character.name} 加入战斗");
	}
}
