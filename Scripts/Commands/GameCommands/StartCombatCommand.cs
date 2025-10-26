using RealismCombat.Nodes;
namespace RealismCombat.Commands.GameCommands;
class StartCombatCommand(ProgramRoot root) : Command(root)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		root.McpCheckPoint();
	}
}
