using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Nodes;
namespace RealismCombat.StateMachine;
class GameState : State
{
	public readonly Game game;
	public GameState(ProgramRoot root) : base(root: root, owner: root) => game = new();
	private protected override void OnExit() => throw new NotImplementedException();
	private protected override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartCombatCommand.name] = arguments => new StartCombatCommand(root: root),
		};
	private protected override string GetStatus() => "游戏中";
}
