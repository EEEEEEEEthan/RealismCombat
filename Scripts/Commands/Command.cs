using System.Collections.Generic;
namespace RealismCombat.Commands;
public abstract class Command
{
	public readonly ProgramRoot programRoot;
	public readonly IReadOnlyDictionary<string, string> arguments;
	protected Command(ProgramRoot programRoot, IReadOnlyDictionary<string, string>? arguments = null)
	{
		this.programRoot = programRoot;
		this.arguments = arguments ?? new Dictionary<string, string>();
	}
	protected Command(ProgramRoot programRoot, string command)
	{
		var parts = command.Split(" ");
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		this.programRoot = programRoot;
		this.arguments = arguments;
	}
	public abstract void Execute();
}
