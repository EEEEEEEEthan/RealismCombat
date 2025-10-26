namespace RealismCombat.Commands;
class ShutdownCommand(ProgramRoot root) : Command(root)
{
	public const string name = "system_shutdown";
	public override void Execute()
	{
		root.CallDeferred(ProgramRoot.MethodName._QuitGame);
		Log.Print("游戏即将关闭");
		root.McpCheckPoint();
	}
}
