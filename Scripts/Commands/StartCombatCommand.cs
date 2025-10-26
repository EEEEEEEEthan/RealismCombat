namespace RealismCombat.Commands;
public class StartCombatCommand(ProgramRoot programRoot) : Command(programRoot)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		if (programRoot.battlePrepareScene != null)
		{
			programRoot.battlePrepareScene.QueueFree();
			programRoot.battlePrepareScene = null;
		}
		var combat = new Combat(programRoot);
		programRoot.combat = combat;
		var character1 = new Character(combat: combat, name: "角色1", team: 0);
		combat.AddCharacter(character1);
		var character2 = new Character(combat: combat, name: "角色2", team: 1);
		combat.AddCharacter(character2);
		_ = new ProgramRoot.CombatState(programRoot);
		programRoot.McpCheckPoint();
	}
}
