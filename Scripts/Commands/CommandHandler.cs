using System.Collections.Generic;
namespace RealismCombat.Commands;
/// <summary>
///     负责处理文本命令。
/// </summary>
public class CommandHandler(GameRoot gameRoot)
{
	public void Handle(string command)
	{
		var parts = command.Split(' ');
		var name = parts[0];
		var args = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length - 1; i += 2) args[parts[i]] = args[parts[i + 1]];
		switch (command)
		{
			case CheckStatus.name:
				new CheckStatus(gameRoot, args).Execute();
				break;
			case StartNextCombat.name:
				new StartNextCombat(gameRoot, args).Execute();
				break;
			default:
				Log.Print("unknown command: " + command);
				gameRoot.McpCheckPoint();
				break;
		}
	}
}
public abstract class Command(GameRoot gameRoot, Dictionary<string, string> args)
{
	protected readonly GameRoot gameRoot = gameRoot;
	protected readonly IReadOnlyDictionary<string, string> args = args ?? new Dictionary<string, string>();
	public abstract void Execute();
}
