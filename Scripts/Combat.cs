using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.StateMachine;
namespace RealismCombat;
/// <summary>
///     战斗核心对象（占位），仅负责承载战斗期的上下文。
/// </summary>
class Combat : IStateOwner
{
	/// <summary>
	///     角色行动状态
	/// </summary>
	public class CharacterActionState(Combat combat, Character character) : State(combat)
	{
		public readonly Character character = character;
		public override string Status =>
			$"""
			{character.name}的回合
			""";
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments) { }
	}
	/// <summary>
	///     回合进行状态
	/// </summary>
	public class RoundProgressState(Combat combat) : State(combat)
	{
		public override string Status =>
			$"""
			回合进行中
			可用指令: {CheckStatusCommand.name}, {ShutdownCommand.name}, {DebugShowNodeTreeCommand.name}
			""";
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments)
		{
			Command command = name switch
			{
				CheckStatusCommand.name => new CheckStatusCommand(combat.programRoot),
				ShutdownCommand.name => new ShutdownCommand(combat.programRoot),
				DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(programRoot: combat.programRoot, arguments: arguments),
				_ => throw new ArgumentException($"当前状态无法执行{name}"),
			};
			command.Execute();
		}
	}
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
	public Combat(ProgramRoot programRoot)
	{
		this.programRoot = programRoot;
		state = new RoundProgressState(this);
	}
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
