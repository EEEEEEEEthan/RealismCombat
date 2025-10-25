namespace RealismCombat.Commands;
/// <summary>
///     负责处理文本命令。
/// </summary>
public class CommandHandler(GameRoot gameRoot)
{
	public void Handle(string command)
	{
		switch (command)
		{
			case CheckStatus.name:
				new CheckStatus(gameRoot).Execute();
				break;
			case StartNextCombat.name:
				new StartNextCombat(gameRoot).Execute();
				break;
		default:
			Log.Print("unknown command: " + command);
			gameRoot.McpCheckPoint();
			break;
		}
	}
}
public abstract class Command(GameRoot gameRoot)
{
	protected readonly GameRoot gameRoot = gameRoot;
	public abstract void Execute();
}
