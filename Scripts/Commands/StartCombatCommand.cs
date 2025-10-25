namespace RealismCombat.Commands;
public class StartCombatCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		if (gameRoot.battlePrepareScene != null)
		{
			gameRoot.battlePrepareScene.QueueFree();
			gameRoot.battlePrepareScene = null;
		}
		gameRoot.State = new GameRoot.CombatState(gameRoot);
		gameRoot.mcpHandler?.McpCheckPoint();
	}
}
