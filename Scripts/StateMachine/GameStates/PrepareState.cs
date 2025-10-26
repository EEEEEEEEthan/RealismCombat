using System;
using System.Collections.Generic;
using RealismCombat.Commands;
using RealismCombat.Commands.GameCommands;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.StateMachine.GameStates;
class PrepareState : State
{
	readonly BattlePrepareScene prepareScene;
	public PrepareState(GameState gameState) : base(root: gameState.root, owner: gameState) => prepareScene = BattlePrepareScene.Create(gameState.root);
	private protected override void OnExit() => prepareScene.QueueFree();
	public override IReadOnlyDictionary<string, Func<IReadOnlyDictionary<string, string>, Command>> GetCommandGetters() =>
		new Dictionary<string, Func<IReadOnlyDictionary<string, string>, Command>>
		{
			[StartCombatCommand.name] = _ => new StartCombatCommand(root),
		};
	private protected override string GetStatus() => "准备状态";
}
