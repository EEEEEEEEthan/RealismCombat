using System.Collections.Generic;
namespace RealismCombat.Commands;
public class CheckStatus(GameRoot gameRoot, Dictionary<string, string> args) : Command(gameRoot: gameRoot, args: args)
{
	public const string name = "game.check_status";
	public override void Execute()
	{
		Log.Print("当前游戏状态: 准备阶段");
		gameRoot.McpCheckPoint();
	}
}
