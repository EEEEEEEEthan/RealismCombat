using System.Collections.Generic;
using System.Threading.Tasks;
namespace RealismCombat.Game.Commands;
/// <summary>
///     负责处理文本命令。
/// </summary>
public class CommandHandler(GameRoot gameRoot)
{
	readonly List<string> _logs = new();
	public async Task<string> Handle(string command)
	{
		_logs.Clear();
		gameRoot.OnLog += OnLog;
		if (command == "game.check_status")
			Log.Print("这个功能还没实现");
		else if (command == StartNextCombat.NAME)
			await new StartNextCombat(gameRoot).Execute();
		else
			Log.Print("unknown command: " + command);
		gameRoot.OnLog -= OnLog;
		return string.Join(separator: "\n", values: _logs);
	}
	void OnLog(string msg) => _logs.Add(msg);
}
