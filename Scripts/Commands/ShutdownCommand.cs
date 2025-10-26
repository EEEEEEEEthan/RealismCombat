namespace RealismCombat.Commands;
public class ShutdownCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "system_shutdown";
	public override void Execute()
	{
		gameRoot.CallDeferred(GameRoot.MethodName._QuitGame);
		Log.Print("游戏即将关闭");
		gameRoot.McpCheckPoint();
	}
}
