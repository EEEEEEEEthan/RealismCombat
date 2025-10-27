using RealismCombat.Data;
using RealismCombat.Nodes;
using RealismCombat.StateMachine.ProgramStates;
using System.Threading.Tasks;
namespace RealismCombat.Commands.ProgramCommands;
class LoadGameCommand(ProgramRootNode rootNode) : Command(rootNode)
{
	public const string name = "program_load_game";
	public override bool Validate(out string error)
	{
		error = "";
		return true;
	}
    public override Task Execute()
	{
		Log.Print("读取游戏");
		var data = Persistant.Load(Persistant.saveDataPath);
		_ = new GameState(rootNode: rootNode, gameData: data);
		rootNode.McpCheckPoint();
        return Task.CompletedTask;
    }
}
