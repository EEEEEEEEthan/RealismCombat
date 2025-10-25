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
	static string system_start_game()
	{
		using var _ = Log.BeginScope(out var builder);
		Log.Print("收到启动游戏请求");
		if (Client != null)
		{
			Log.Print($"游戏已在运行中 - 端口: {Client.port}, 进程ID: {Client.ProcessId}");
			return builder.ToString();
		}
		try
		{
			Log.Print("正在创建游戏客户端实例...");
			var client = new GameClient();
			client.OnDisconnected += () =>
			{
				Log.Print("游戏连接断开，自动清理GameClient");
				client.Dispose();
				Client = null;
			};
			Client = client;
			Log.Print($"游戏启动成功\n端口: {Client.port}\n进程ID: {Client.ProcessId}\n日志文件: {Client.logFilePath}");
			return builder.ToString();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			Client?.Dispose();
			Client = null;
			return builder.ToString();
		}
	}
	[McpServerTool, Description("stop game"),]
	static string system_shutdown()
	{
		using var _ = Log.BeginScope(out var builder);
		Log.Print("收到停止游戏请求");
		try
		{
			if (Client is null)
			{
				Log.Print("游戏未运行，无需停止");
				return "not running";
			}
			Log.Print("正在发送关闭命令到游戏...");
			var result = Client.SendCommand(nameof(system_shutdown), 3000).GetAwaiter().GetResult();
			Log.Print($"游戏关闭命令已发送，结果: {result}");
			Client.Dispose();
			Client = null;
			Log.Print("游戏客户端已释放");
			return builder.ToString();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			Client?.Dispose();
			Client = null;
			return builder.ToString();
		}
	}
}
[McpServerToolType]
static class GameTools
{
	[McpServerTool, Description("check current status"),]
	static Task<string> game_check_status()
	{
		if (SystemTools.Client is null) return Task.FromResult("游戏未启动. 使用start_game启动游戏");
		return SystemTools.Client.SendCommand(nameof(game_check_status), 3000);
	}
	[McpServerTool, Description("start next combat"),]
	static Task<string> game_start_combat()
	{
		if (SystemTools.Client is null) return Task.FromResult("游戏未启动. 使用start_game启动游戏");
		return SystemTools.Client.SendCommand(nameof(game_start_combat), 3000);
	}
}
