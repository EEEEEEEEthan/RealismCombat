using System.Collections.Generic;
using System.Threading.Tasks;
using RealismCombat.Nodes;
namespace RealismCombat.Commands;
abstract class Command
{
	public readonly ProgramRootNode rootNode;
	public readonly IReadOnlyDictionary<string, string> arguments;
	protected Command(ProgramRootNode rootNode, IReadOnlyDictionary<string, string>? arguments = null)
	{
		this.rootNode = rootNode;
		this.arguments = arguments ?? new Dictionary<string, string>();
	}
	protected Command(ProgramRootNode rootNode, string command)
	{
		var parts = command.Split(" ");
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		this.rootNode = rootNode;
		this.arguments = arguments;
	}
    public abstract Task Execute();
}
