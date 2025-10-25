using System.Collections.Generic;
namespace RealismCombat.Commands;
public class StartCombatCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game.start_combat";
	public override void Execute(IReadOnlyDictionary<string, string> arguments)
	{
		Log.Print("战斗开始!");
		gameRoot.mcpHandler?.McpCheckPoint();
	}
}
