using System.Threading.Tasks;
namespace RealismCombat.Game.Commands;
/// <summary>
///     开始下一场战斗。
/// </summary>
public class StartNextCombat
{
	public const string NAME = "game.start_next_combat";
	readonly GameRoot gameRoot;
	public StartNextCombat(GameRoot gameRoot) => this.gameRoot = gameRoot;
	public Task Execute()
	{
		Log.Print("战斗开始了");
		return Task.CompletedTask;
	}
}
