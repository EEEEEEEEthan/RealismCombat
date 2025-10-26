using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.ProgramCommands;
class StartNewGameCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "program_start_new_game";
	public override void Execute()
	{
		Log.Print("开始新游戏");
		_ = new GameState(rootNode);
		rootNode.McpCheckPoint();
	}
}
