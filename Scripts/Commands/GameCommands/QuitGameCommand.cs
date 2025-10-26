using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class QuitGameCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "game_quit_to_menu";
	public override void Execute()
	{
		_ = new MenuState(rootNode);
		rootNode.McpCheckPoint();
	}
}
