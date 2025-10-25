using Godot;
namespace RealismCombat;
public class CommandHandler(GameRoot gameRoot)
{
	public void HandleCommand(string cmd)
	{
		switch (cmd)
		{
			case "system.shutdown":
				gameRoot.CallDeferred(GameRoot.MethodName._QuitGame);
				Log.Print("游戏即将关闭");
				gameRoot.McpHandler?.McpCheckPoint();
				break;
			case "game.check_status":
				PrintStatus();
				gameRoot.McpHandler?.McpCheckPoint();
				break;
			default:
				Log.Print($"未知指令{cmd}");
				break;
		}
	}
	void PrintStatus()
	{
		Log.Print("游戏运行状态:");
		Log.Print($"  运行时间: {gameRoot.TotalTime:F2}秒");
		Log.Print($"  总帧数: {gameRoot.FrameCount}");
		Log.Print($"  平均FPS: {(gameRoot.TotalTime > 0 ? gameRoot.FrameCount / gameRoot.TotalTime : 0):F2}");
		Log.Print($"  当前FPS: {Engine.GetFramesPerSecond()}");
		Log.Print($"  客户端已连接: {gameRoot.HadClientConnected}");
		if (gameRoot.McpHandler is not null)
			Log.Print("  服务器状态: 运行中");
		else
			Log.Print("  服务器状态: 未启动");
	}
}
