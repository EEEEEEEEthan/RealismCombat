using RealismCombat.Nodes;
namespace RealismCombat.Commands.ProgramCommands;
class ShutdownCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "prgram_shutdown";
	public override void Execute()
	{
		rootNode.CallDeferred(ProgramRootNode.MethodName._QuitGame);
		Log.Print("游戏即将关闭");
		rootNode.McpCheckPoint();
	}
}
