using System.Collections.Generic;
using System.Linq;
namespace RealismCombat.StateMachine;
public interface IStateOwner
{
	State State { get; set; }
}
public abstract class State(IStateOwner owner)
{
	public readonly IStateOwner owner = owner;
	IReadOnlyList<string>? availableCommands;
	public IReadOnlyList<string> AvailableCommands => availableCommands ??= GetAvailableCommands().ToList();
	bool Expired => owner.State != this;
	public void ExecuteCommand(string command)
	{
		var parts = command.Split(" ");
		var name = parts[0];
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		if (!AvailableCommands.Contains(name)) throw new($"状态机当前状态不支持指令{name}");
		ExecuteCommand(name: name, arguments: arguments);
	}
	protected abstract void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments);
	private protected abstract IEnumerable<string> GetAvailableCommands();
}
