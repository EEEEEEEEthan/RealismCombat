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
}
