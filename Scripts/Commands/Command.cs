using System.Collections.Generic;
namespace RealismCombat.Commands;
public abstract class Command(GameRoot gameRoot)
{
	public readonly GameRoot gameRoot = gameRoot;
	public abstract void Execute(IReadOnlyDictionary<string, string> arguments);
}
