namespace RealismCombat.Commands;
class ShutdownCommand(ProgramRoot programRoot) : Command(programRoot)
{
	public const string name = "system_shutdown";
	public override void Execute()
	{
		programRoot.CallDeferred(ProgramRoot.MethodName._QuitGame);
		Log.Print("游戏即将关闭");
		programRoot.McpCheckPoint();
	}
}
