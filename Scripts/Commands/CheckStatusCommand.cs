using System.Collections.Generic;
using Godot;
namespace RealismCombat.Commands;
public class CheckStatusCommand(GameRoot gameRoot) : Command(gameRoot)
{
	public const string name = "game.check_status";
	public override void Execute(IReadOnlyDictionary<string, string> arguments)
	{
		PrintStatus();
		gameRoot.mcpHandler?.McpCheckPoint();
	}
	void PrintStatus()
	{
		Log.Print("游戏运行状态:");
		Log.Print($"  运行时间: {gameRoot.TotalTime:F2}秒");
		Log.Print($"  总帧数: {gameRoot.FrameCount}");
		Log.Print($"  平均FPS: {(gameRoot.TotalTime > 0 ? gameRoot.FrameCount / gameRoot.TotalTime : 0):F2}");
		Log.Print($"  当前FPS: {Engine.GetFramesPerSecond()}");
		Log.Print($"  客户端已连接: {gameRoot.HadClientConnected}");
		if (gameRoot.mcpHandler is not null)
			Log.Print("  服务器状态: 运行中");
		else
			Log.Print("  服务器状态: 未启动");
	}
}

