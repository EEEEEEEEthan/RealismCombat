using System;
using System.Collections.Concurrent;
using Godot;
using RealismCombat.McpServer;
namespace RealismCombat.Nodes;
/// <summary>
///     程序根节点，负责初始化MCP服务器并处理游戏生命周期
/// </summary>
public partial class ProgramRootNode : Node
{
	static int? ParsePortFromCommandLine()
	{
		var args = OS.GetCmdlineUserArgs();
		Log.Print($"[ProgramRoot] 用户命令行参数: {string.Join(", ", args)}");
		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];
			if (arg.StartsWith("--port="))
			{
				var portStr = arg["--port=".Length..];
				if (int.TryParse(portStr, out var parsedPort)) return parsedPort;
				Log.PrintErr($"[ProgramRoot] 无效的端口参数: {portStr}");
			}
		}
		return null;
	}
	readonly ConcurrentQueue<(string command, Action<string> respond)> commandQueue = new();
	GameServer? server;
	int? port;
	bool shouldQuitOnDisconnect;
	public override void _Ready()
	{
		Log.Print("[ProgramRoot] 程序启动");
		var godotPath = Settings.Get("godot");
		if (godotPath != null) Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
		port = ParsePortFromCommandLine();
		if (port.HasValue)
		{
			Log.Print($"[ProgramRoot] 从命令行参数获取端口: {port.Value}");
			shouldQuitOnDisconnect = true;
			StartServer(port.Value);
		}
		else
		{
			Log.Print("[ProgramRoot] 未指定端口，以普通模式运行");
		}
	}
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var item)) HandleCommand(item.command, item.respond);
	}
	public override void _ExitTree()
	{
		Log.Print("[ProgramRoot] 程序退出");
		server?.Dispose();
		server = null;
	}
	void StartServer(int serverPort)
	{
		try
		{
			server = new(serverPort);
			server.OnConnected += () => { Log.Print("[ProgramRoot] MCP客户端已连接"); };
			server.OnDisconnected += () =>
			{
				Log.Print("[ProgramRoot] MCP客户端已断开");
				if (shouldQuitOnDisconnect)
				{
					Log.Print("[ProgramRoot] 客户端断开，准备退出游戏...");
					CallDeferred(Node.MethodName.GetTree).AsGodotObject().Call("quit");
				}
			};
			server.OnCommandReceived += (command, respond) => { commandQueue.Enqueue((command, respond)); };
			server.Start();
			Log.Print($"[ProgramRoot] MCP服务器已启动，端口: {serverPort}");
		}
		catch (Exception ex)
		{
			Log.PrintErr($"[ProgramRoot] 启动服务器失败: {ex}");
			GetTree().Quit(1);
		}
	}
	void HandleCommand(string command, Action<string> respond)
	{
		Log.Print($"[ProgramRoot] 处理命令: {command}");
		try
		{
			switch (command)
			{
				case "system_launch_program":
					respond("游戏已启动并连接成功");
					break;
				case "ping":
					respond("pong");
					break;
				default:
					Log.PrintErr($"[ProgramRoot] 未知命令: {command}");
					respond($"未知命令: {command}");
					break;
			}
		}
		catch (Exception ex)
		{
			Log.PrintErr($"[ProgramRoot] 处理命令异常: {ex}");
			respond($"错误: {ex.Message}");
		}
	}
}
