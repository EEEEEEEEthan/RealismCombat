using System.Collections.Generic;
namespace RealismCombat.Commands;
public abstract class Command
{
	public readonly GameRoot gameRoot;
	public readonly IReadOnlyDictionary<string, string> arguments;
	protected Command(GameRoot gameRoot, IReadOnlyDictionary<string, string>? arguments = null)
	{
		this.gameRoot = gameRoot;
		this.arguments = arguments ?? new Dictionary<string, string>();
	}
	protected Command(GameRoot gameRoot, string command)
	{
		var parts = command.Split(" ");
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		this.gameRoot = gameRoot;
		this.arguments = arguments;
	}
	public abstract void Execute();
}
