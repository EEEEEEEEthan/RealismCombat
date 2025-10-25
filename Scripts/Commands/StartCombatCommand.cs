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
		var combat = new Combat(gameRoot);
		gameRoot.combat = combat;

		var character1 = new Character(combat: combat, name: "角色1", team: 0);
		combat.AddCharacter(character1);

		var character2 = new Character(combat: combat, name: "角色2", team: 1);
		combat.AddCharacter(character2);

		gameRoot.State = new GameRoot.CombatState(gameRoot);
		gameRoot.mcpHandler?.McpCheckPoint();
	}
}
