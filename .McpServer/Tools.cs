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
	[McpServerTool, Description("launch program"),]
	static async Task<string> system_launch_program()
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
			builder.AppendLine(await Client.SendCommand("system_launch_program", 3000));
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
	[McpServerTool, Description("shutdown program"),]
	static Task<string> system_shutdown()
	{
		using var _ = Log.BeginScope(out var builder);
		Log.Print("收到关闭程序请求");
		if (Client == null)
		{
			Log.Print("程序未在运行中");
			return Task.FromResult(builder.ToString());
		}
		try
		{
			Log.Print("正在关闭程序...");
			Client.Dispose();
			Client = null;
			Log.Print("程序已关闭");
			return Task.FromResult(builder.ToString());
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			return Task.FromResult(builder.ToString());
		}
	}
}
[McpServerToolType]
static class DebugTools
{
	[McpServerTool, Description("get scene tree in JSON format"),]
	static async Task<string> debug_get_scene_tree()
	{
		using var _ = Log.BeginScope(out var builder);
		Log.Print("收到获取场景树请求");
		if (SystemTools.Client == null)
		{
			Log.Print("程序未在运行中");
			return builder.ToString();
		}
		try
		{
			var response = await SystemTools.Client.SendCommand("debug_get_scene_tree", 3000);
			builder.AppendLine(response);
			return builder.ToString();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			return builder.ToString();
		}
	}
}
