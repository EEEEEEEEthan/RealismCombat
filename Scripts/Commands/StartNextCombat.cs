using System.Collections.Generic;
namespace RealismCombat.Commands;
/// <summary>
///     开始下一场战斗。
/// </summary>
public class StartNextCombat(GameRoot gameRoot, Dictionary<string, string> args) : Command(gameRoot: gameRoot, args: args)
{
	public const string name = "game.start_next_combat";
	public override void Execute()
	{
		Log.Print("战斗开始了");
		gameRoot.McpCheckPoint();
	}
}
