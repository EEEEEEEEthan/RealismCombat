using System;
using RealismCombat.Data;
using RealismCombat.Extensions;
using RealismCombat.StateMachine.GameStates;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class StartCombatCommand(GameState gameState) : GameCommand(gameState)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		var combatData = new CombatData();
		combatData.characters.Add(new(name: "ethan", team: 0, actionPoint: Random.Shared.NextSingle().Remapped(fromMin: 0, fromMax: 1, toMin: -10, toMax: 0)));
		combatData.characters.Add(new(name: "dove", team: 1, actionPoint: Random.Shared.NextSingle().Remapped(fromMin: 0, fromMax: 1, toMin: -10, toMax: 0)));
		_ = new CombatState(gameState: gameState, combatData: combatData);
		gameState.Save();
		rootNode.McpCheckPoint();
	}
}
