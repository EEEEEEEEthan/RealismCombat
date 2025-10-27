using RealismCombat.StateMachine.ProgramStates;
using System.Threading.Tasks;
namespace RealismCombat.Commands.GameCommands;
class QuitGameCommand(GameState gameState) : GameCommand(gameState)
{
	public const string name = "game_quit_to_menu";
	public override bool Validate(out string error)
	{
		error = "";
		return true;
	}
    public override Task Execute()
	{
		gameState.Save();
		_ = new MenuState(rootNode);
		rootNode.McpCheckPoint();
        return Task.CompletedTask;
    }
}
