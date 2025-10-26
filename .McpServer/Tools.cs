// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
	public const string tool_launch_program = nameof(system_launch_program);
	public static GameClient? Client { get; private set; }
	[McpServerTool, Description("launch program"),]
	static string system_launch_program()
	{
		using var _ = Log.BeginScope(out var builder);
		Log.Print("收到启动程序请求");
		if (Client != null)
		{
			Log.Print($"程序已在运行中 - 端口: {Client.port}, 进程ID: {Client.ProcessId}");
			return builder.ToString();
		}
		try
		{
			Log.Print("正在创建程序客户端实例...");
			var client = new GameClient();
			client.OnDisconnected += () =>
			{
				Log.Print("程序连接断开，自动清理GameClient");
				client.Dispose();
				Client = null;
			};
			Client = client;
			Log.Print($"程序启动成功\n端口: {Client.port}\n进程ID: {Client.ProcessId}\n日志文件: {Client.logFilePath}");
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
		Log.Print("收到停止程序请求");
		try
		{
			if (Client is null)
			{
				Log.Print("程序未运行，无需停止");
				return "not running";
			}
			Log.Print("正在发送关闭命令到程序...");
			var result = Client.SendCommand(nameof(system_shutdown), 3000).GetAwaiter().GetResult();
			Log.Print($"程序关闭命令已发送，结果: {result}");
			Client.Dispose();
			Client = null;
			Log.Print("程序客户端已释放");
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
static class ProgramTools
{
	[McpServerTool, Description("start new game"),]
	static Task<string> program_start_new_game()
	{
		if (SystemTools.Client is null) return Task.FromResult($"程序未启动. 使用{nameof(SystemTools.tool_launch_program)}启动程序");
		return SystemTools.Client.SendCommand(nameof(program_start_new_game), 3000);
	}
}
[McpServerToolType]
static class GameTools
{
	[McpServerTool, Description("start next combat"),]
	static Task<string> game_start_combat()
	{
		if (SystemTools.Client is null) return Task.FromResult($"程序未启动. 使用{nameof(SystemTools.tool_launch_program)}启动程序");
		return SystemTools.Client.SendCommand(nameof(game_start_combat), 3000);
	}
}
[McpServerToolType]
static class DebugTools
{
	[McpServerTool, Description("show node tree structure in json format"),]
	static Task<string> debug_show_node_tree(string root = "/root")
	{
		if (SystemTools.Client is null) return Task.FromResult($"程序未启动. 使用{nameof(SystemTools.tool_launch_program)}启动程序");
		return SystemTools.Client.SendCommand($"{nameof(debug_show_node_tree)} root {root}", 3000);
	}
}
