using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Nodes;
namespace RealismCombat.StateMachine;
interface IStateOwner
{
	State State { get; set; }
}
abstract class State
{
	public readonly ProgramRoot root;
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
	public State(ProgramRoot root, IStateOwner owner)
	{
		this.root = root;
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
		owner.State = this;
	}
	public void ExecuteCommand(string command)
	{
		var parts = command.Split(" ");
		var name = parts[0];
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		var cmd = name switch
		{
			ShutdownCommand.name => new ShutdownCommand(root),
			DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(root: root, arguments: arguments),
			_ => GetCommandGetters()[name](arguments),
		};
		cmd.Execute();
	}
	public virtual void Update(double dt) { }
	private protected abstract void OnExit();
	private protected abstract IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters();
	private protected abstract string GetStatus();
}
