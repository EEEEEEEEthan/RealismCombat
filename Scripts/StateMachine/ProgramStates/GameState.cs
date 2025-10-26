using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.GameCommands;
using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.GameStates;
namespace RealismCombat.StateMachine.ProgramStates;
class GameState : State, IStateOwner
{
	public readonly GameNode gameNode;
	readonly GameData gameData;
	State state = null!;
	public State State
	{
		get => state;
		private set
		{
			state = value;
			gameData.state = state switch
			{
				PrepareState => 0,
				CombatState => 1,
				_ => throw new($"unexpected state type: {state.GetType()}"),
			};
		}
	}
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	public GameState(ProgramRootNode rootNode) : base(rootNode: rootNode, owner: rootNode)
	{
		gameData = new();
		gameNode = GameNode.Create(this);
		rootNode.AddChild(gameNode);
		State = new PrepareState(this);
	}
	public GameState(ProgramRootNode rootNode, GameData gameData) : base(rootNode: rootNode, owner: rootNode)
	{
		this.gameData = gameData;
		gameNode = GameNode.Create(this);
		rootNode.AddChild(gameNode);
		State = gameData.state switch
		{
			0 => new PrepareState(this),
			1 => new CombatState(this),
			_ => throw new($"unexpected state id: {gameData.state}"),
		};
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters()
	{
		var dict = new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>(State.GetCommandGetters())
		{
			[QuitGameCommand.name] = _ => new QuitGameCommand(this),
		};
		return dict;
	}
	public override void Update(double dt) => State.Update(dt);
	private protected override void OnExit() => gameNode.QueueFree();
	private protected override string GetStatus() => "游戏中";
}
