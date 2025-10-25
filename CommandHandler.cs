using System.Text;
using Godot;
namespace RealismCombat;
public class CommandHandler(GameRoot gameRoot)
{
	public string HandleCommand(string cmd)
	{
		switch (cmd)
		{
			case "system.shutdown":
				gameRoot.CallDeferred(GameRoot.MethodName._QuitGame);
				return "ok";
			case "game.check_status":
				return BuildStatusResponse();
			default:
				return "unknown";
		}
	}
	string BuildStatusResponse()
	{
		var sb = new StringBuilder();
		sb.AppendLine("游戏运行状态:");
		sb.AppendLine($"  运行时间: {gameRoot.TotalTime:F2}秒");
		sb.AppendLine($"  总帧数: {gameRoot.FrameCount}");
		sb.AppendLine($"  平均FPS: {(gameRoot.TotalTime > 0 ? gameRoot.FrameCount / gameRoot.TotalTime : 0):F2}");
		sb.AppendLine($"  当前FPS: {Engine.GetFramesPerSecond()}");
		sb.AppendLine($"  客户端已连接: {gameRoot.HadClientConnected}");
		if (gameRoot.Server is not null)
		{
			sb.AppendLine("  服务器状态: 运行中");
			sb.AppendLine($"  待处理请求: {(gameRoot.Server.PendingRequest is not null ? "有" : "无")}");
		}
		else
		{
			sb.AppendLine("  服务器状态: 未启动");
		}
		return sb.ToString().TrimEnd();
	}
}
