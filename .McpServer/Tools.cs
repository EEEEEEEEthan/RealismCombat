// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
	public static GameClient? Client { get; private set; }
	[McpServerTool, Description("start game"),]
	static string start_game()
	{
		Log.Print("收到启动游戏请求");
		if (Client != null)
		{
			var msg = $"游戏已在运行中\n端口: {Client.port}\n进程ID: {Client.ProcessId}\n日志文件: {Client.logFilePath}";
			Log.Print($"游戏已在运行中 - 端口: {Client.port}, 进程ID: {Client.ProcessId}");
			return msg;
		}
		try
		{
			Log.Print("正在创建游戏客户端实例...");
			Client = new();
			var msg = $"游戏启动成功\n端口: {Client.port}\n进程ID: {Client.ProcessId}\n日志文件: {Client.logFilePath}";
			Log.Print($"游戏启动成功 - 端口: {Client.port}, 进程ID: {Client.ProcessId}, 日志: {Client.logFilePath}");
			return msg;
		}
		catch (Exception e)
		{
			Log.PrintError("游戏启动失败", e);
			Client?.Dispose();
			Client = null;
			return $"启动失败: {e.Message}";
		}
	}
	[McpServerTool, Description("stop game"),]
	static string stop_game()
	{
		Log.Print("收到停止游戏请求");
		try
		{
			if (Client is null)
			{
				Log.Print("游戏未运行，无需停止");
				return "not running";
			}
			Log.Print("正在发送关闭命令到游戏...");
			var result = Client.SendCommand("system.shutdown", 3000).GetAwaiter().GetResult();
			Log.Print($"游戏关闭命令已发送，结果: {result}");
			Client?.Dispose();
			Client = null;
			Log.Print("游戏客户端已释放");
			return result;
		}
		catch (Exception e)
		{
			Log.PrintError("停止游戏时发生错误", e);
			Client?.Dispose();
			Client = null;
			return $"error: {e.Message}";
		}
	}
}
/*
[McpServerToolType]
static class GameTools
{
	[McpServerTool, Description("check current status"),]
	static Task<string> status()
	{
		if (SystemTools.Client is null) return Task.FromResult("未连接到游戏");
		return SystemTools.Client.SendCommand("game.check_status", 3000);
	}
	[McpServerTool, Description("start next combat"),]
	static Task<string> start_next_combat()
	{
		if (SystemTools.Client is null) return Task.FromResult("未连接到游戏");
		return SystemTools.Client.SendCommand("game.start_next_combat", 3000);
	}
}
*/
