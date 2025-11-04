using System;
using System.Collections.Concurrent;
using System.Text.Json;
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
	bool isQuitting;
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
		GetTree().Root.CloseRequested += OnCloseRequested;
	}
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var item)) HandleCommand(item.command, item.respond);
	}
	public override void _ExitTree()
	{
		Log.Print("[ProgramRoot] 程序退出，开始清理");
		try
		{
			server?.Dispose();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		server = null;
		Log.Print("[ProgramRoot] 程序退出完成");
	}
	void OnCloseRequested()
	{
		Log.Print("[ProgramRoot] 收到窗口关闭请求，强制退出");
		System.Environment.Exit(0);
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
				if (shouldQuitOnDisconnect && !isQuitting)
				{
					isQuitting = true;
					Log.Print("[ProgramRoot] 客户端断开，准备退出游戏...");
					GetTree().CallDeferred(SceneTree.MethodName.Quit);
				}
			};
			server.OnCommandReceived += (command, respond) => { commandQueue.Enqueue((command, respond)); };
			server.Start();
			Log.Print($"[ProgramRoot] MCP服务器已启动，端口: {serverPort}");
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
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
				case "system_shutdown":
					respond("正在关闭游戏");
					GetTree().CallDeferred(SceneTree.MethodName.Quit);
					break;
				case "debug_get_scene_tree":
					var treeJson = GetSceneTreeJson();
					respond(treeJson);
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
			Log.PrintException(ex);
			respond($"错误: {ex.Message}");
		}
	}
	string GetSceneTreeJson()
	{
		var root = GetTree().Root;
		var treeDict = BuildNodeTree(root);
		return JsonSerializer.Serialize(treeDict, new JsonSerializerOptions { WriteIndented = true, });
	}
	object BuildNodeTree(Node node)
	{
		var children = node.GetChildren();
		if (children.Count == 0) return new { };
		var childrenDict = new System.Collections.Generic.Dictionary<string, object>();
		foreach (var child in children) childrenDict[child.Name] = BuildNodeTree(child);
		return childrenDict;
	}
}
