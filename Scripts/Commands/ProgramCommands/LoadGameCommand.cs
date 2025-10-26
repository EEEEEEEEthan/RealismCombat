using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Commands.ProgramCommands;
class LoadGameCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "program_load_game";
	public override void Execute()
	{
		Log.Print("读取游戏");
		var data = Persistant.Load(Persistant.saveDataPath);
		_ = new GameState(rootNode: rootNode, gameData: data);
		rootNode.McpCheckPoint();
	}
}
