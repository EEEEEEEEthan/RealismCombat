using System.Collections.Generic;
using System.Linq;
namespace RealismCombat.StateMachine;
public interface IStateOwner
{
	State State { get; set; }
}
public abstract class State
{
	public readonly IStateOwner owner;
	public abstract string Status { get; }
	bool Expired => owner.State != this;
	public State(IStateOwner owner)
	{
		this.owner = owner;
		owner.State = this;
	}
	public void ExecuteCommand(string command)
	{
		var parts = command.Split(" ");
		var name = parts[0];
		var arguments = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) arguments[parts[i]] = parts[i + 1];
		ExecuteCommand(name: name, arguments: arguments);
	}
	protected abstract void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments);
}
