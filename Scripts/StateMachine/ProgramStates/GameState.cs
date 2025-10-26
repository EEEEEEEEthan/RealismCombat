using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.GameCommands;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.ProgramStates;
class GameState : State, IStateOwner
{
	public readonly Game game;
	public State State { get; private set; }
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	public GameState(ProgramRoot root) : base(root: root, owner: root)
	{
		State = new PrepareState(this);
		game = Game.Create(this);
		root.AddChild(game);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters()
	{
		var dict = new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>(State.GetCommandGetters())
		{
			[QuitGameCommand.name] = _ => new QuitGameCommand(root),
		};
		return dict;
	}
	private protected override void OnExit() => game.QueueFree();
	private protected override string GetStatus() => "游戏中";
}
