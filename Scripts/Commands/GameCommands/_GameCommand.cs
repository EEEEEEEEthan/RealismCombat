using System.Collections.Generic;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
abstract class GameCommand : Command
{
	public readonly GameState gameState;
	protected GameCommand(GameState gameState, IReadOnlyDictionary<string, string>? arguments = null) :
		base(rootNode: gameState.rootNode, arguments: arguments) =>
		this.gameState = gameState;
	protected GameCommand(GameState gameState, string command) : base(rootNode: gameState.rootNode, command: command) => this.gameState = gameState;
}
