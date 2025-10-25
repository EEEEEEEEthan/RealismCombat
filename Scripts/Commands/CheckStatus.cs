namespace RealismCombat.Commands;
public class CheckStatus(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game.check_status";
	public override void Execute()
	{
		Log.Print("当前游戏状态: 准备阶段");
		gameRoot.McpCheckPoint();
	}
}
