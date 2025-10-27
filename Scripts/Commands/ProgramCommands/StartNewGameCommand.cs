using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
using System.Threading.Tasks;
namespace RealismCombat.Commands.ProgramCommands;
class StartNewGameCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "program_start_new_game";
	public override bool Validate(out string error)
	{
		error = "";
		return true;
	}
    public override Task Execute()
	{
		Log.Print("开始新游戏");
		_ = new GameState(rootNode);
		rootNode.McpCheckPoint();
        return Task.CompletedTask;
    }
}
