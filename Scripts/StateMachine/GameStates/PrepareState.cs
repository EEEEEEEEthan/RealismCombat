using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.GameCommands;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.StateMachine.GameStates;
class PrepareState : State
{
	readonly GameState gameState;
	readonly Nodes.PrepareNode prepareNode;
	public PrepareState(GameState gameState) : base(rootNode: gameState.rootNode, owner: gameState)
	{
		this.gameState = gameState;
		prepareNode = Nodes.PrepareNode.Create(gameState.rootNode);
		gameState.gameNode.AddChild(prepareNode);
	}
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartCombatCommand.name] = _ => new StartCombatCommand(rootNode),
		};
	private protected override void OnExit() => prepareNode.QueueFree();
	private protected override string GetStatus() => "准备状态";
}
