using System;
using System.Collections.Generic;
using RealismCombat.Commands;
namespace RealismCombat.StateMachine;
class MenuState : State
{
	public MenuState(ProgramRoot root) : base(root: root, owner: root) { }
	private protected override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartNewGameCommand.name] = _ => new StartNewGameCommand(root),
		};
	private protected override string GetStatus() => throw new NotImplementedException();
}
