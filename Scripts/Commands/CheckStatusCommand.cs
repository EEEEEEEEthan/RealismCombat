namespace RealismCombat.Commands;
public class CheckStatusCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game.check_status";
	public override void Execute()
	{
		Log.Print($"当前:{gameRoot.State}");
		Log.Print($"可用指令: {string.Join(separator: ", ", values: gameRoot.State.AvailableCommands)}");
		gameRoot.mcpHandler?.McpCheckPoint();
	}
}
