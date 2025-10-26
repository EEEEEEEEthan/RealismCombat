using RealismCombat.StateMachine.GameStates;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class StartCombatCommand(GameState gameState) : GameCommand(gameState)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		_ = new CombatState(gameState);
		gameState.Save();
		rootNode.McpCheckPoint();
	}
}
