using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class QuitGameCommand(GameState gameState) : GameCommand(gameState)
{
	public const string name = "game_quit_to_menu";
	public override void Execute()
	{
		_ = new MenuState(rootNode);
		rootNode.McpCheckPoint();
	}
}
