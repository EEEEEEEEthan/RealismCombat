using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.DebugCommands;
using RealismCombat.Commands.ProgramCommands;
using RealismCombat.Nodes;
namespace RealismCombat.StateMachine;
interface IStateOwner
{
	State State { get; set; }
}
abstract class State
{
	public readonly ProgramRootNode rootNode;
	public readonly IStateOwner owner;
	public string Status
	{
		get
		{
			var commandGetters = new HashSet<string>(GetCommandGetters().Keys)
			{
				ShutdownCommand.name,
				DebugShowNodeTreeCommand.name,
			};
			var list = new List<string>(commandGetters);
			return $"{GetStatus()}\n可用指令:{string.Join(separator: ", ", values: list)}";
		}
	}
	bool Expired => owner.State != this;
	public State(ProgramRootNode rootNode, IStateOwner owner)
	{
		this.rootNode = rootNode;
		this.owner = owner;
		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (owner.State != null)
			try
			{
				owner.State.OnExit();
			}
			catch (Exception e)
			{
				Log.PrintException(e);
			}
		Log.Print($"{owner}进入{this}状态");
		owner.State = this;
	}
	public void ExecuteCommand(string command)
	{
		if (Expired) throw new InvalidOperationException("状态已过期，无法执行指令");
		var parts = command.Split(" ");
		var name = parts[0];
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		var cmd = name switch
		{
			ShutdownCommand.name => new ShutdownCommand(rootNode),
			DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(rootNode: rootNode, arguments: arguments),
			_ => GetCommandGetters()[name](arguments),
		};
		cmd.Execute();
	}
	public virtual void Update(double dt) { }
	public abstract IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters();
	private protected abstract void OnExit();
	private protected abstract string GetStatus();
}
