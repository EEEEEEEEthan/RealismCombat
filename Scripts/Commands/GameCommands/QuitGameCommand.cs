using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class QuitGameCommand(ProgramRoot root) : Command(root)
{
	public const string name = "game_quit_to_menu";
	public override void Execute()
	{
		_ = new MenuState(root);
		root.McpCheckPoint();
	}
}
