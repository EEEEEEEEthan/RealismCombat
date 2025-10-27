using RealismCombat.Nodes;
using System.Threading.Tasks;
namespace RealismCombat.Commands.ProgramCommands;
class ShutdownCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "program_shutdown";
	public override bool Validate(out string error)
	{
		error = "";
		return true;
	}
    public override Task Execute()
	{
		rootNode.CallDeferred(ProgramRootNode.MethodName._QuitGame);
		Log.Print("游戏即将关闭");
		rootNode.McpCheckPoint();
        return Task.CompletedTask;
    }
}
