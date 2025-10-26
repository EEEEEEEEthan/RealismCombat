using System.Collections.Generic;
namespace RealismCombat.Commands;
abstract class Command
{
	public readonly ProgramRoot root;
	public readonly IReadOnlyDictionary<string, string> arguments;
	protected Command(ProgramRoot root, IReadOnlyDictionary<string, string>? arguments = null)
	{
		this.root = root;
		this.arguments = arguments ?? new Dictionary<string, string>();
	}
	protected Command(ProgramRoot root, string command)
	{
		var parts = command.Split(" ");
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		this.root = root;
		this.arguments = arguments;
	}
	public abstract void Execute();
}
