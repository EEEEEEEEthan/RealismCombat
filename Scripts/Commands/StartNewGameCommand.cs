using RealismCombat.Nodes;
using RealismCombat.StateMachine;
namespace RealismCombat.Commands;
class StartNewGameCommand(ProgramRoot root) : Command(root)
{
	public const string name = "program_start_new_game";
	public override void Execute()
	{
		Log.Print("开始新游戏");
		_ = new GameState(root);
		root.McpCheckPoint();
	}
}
