using RealismCombat.Nodes;
using RealismCombat.StateMachine.GameStates;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.GameCommands;
class StartCombatCommand(ProgramRoot root) : Command(root)
{
	public const string name = "game_start_combat";
	public override void Execute()
	{
		Log.Print("战斗开始!");
		if (root.State is GameState gameState)
		{
			_ = new CombatState(gameState);
			root.McpCheckPoint();
		}
	}
}
