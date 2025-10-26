namespace RealismCombat.Commands;
class StartCombatCommand(Nodes.ProgramRoot root) : Command(root)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		root.McpCheckPoint();
	}
}
