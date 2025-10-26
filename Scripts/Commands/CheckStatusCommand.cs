namespace RealismCombat.Commands;
public class CheckStatusCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game_check_status";
	public override void Execute()
	{
		Log.Print(gameRoot.State.Status);
		gameRoot.McpCheckPoint();
	}
}
