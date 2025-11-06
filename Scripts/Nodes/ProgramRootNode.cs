using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Godot;
namespace RealismCombat.Nodes;
/// <summary>
///     程序根节点，负责初始化MCP服务器并处理游戏生命周期
/// </summary>
public partial class ProgramRootNode : Node
{
	readonly ConcurrentQueue<(string command, Action<string> respond)> commandQueue = new();
	bool isQuitting;
	public override void _Ready()
	{
		Log.Print("[ProgramRoot] 程序启动");
		var godotPath = Settings.Get("godot");
		if (godotPath != null) Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
		if (LaunchArgs.port.HasValue) SetupServerCallbacks();
	}
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var item)) HandleCommand(item.command, item.respond);
	}
	public override void _ExitTree() => Log.Print("[ProgramRoot] 程序退出完成");
	void SetupServerCallbacks()
	{
		var server = GetNode<AutoLoad.GameServer>("/root/GameServer");
		server.OnConnected += () => { Log.Print("[ProgramRoot] MCP客户端已连接"); };
		server.OnDisconnected += () => { Log.Print("[ProgramRoot] MCP客户端已断开"); };
		server.OnCommandReceived += (command, respond) => { commandQueue.Enqueue((command, respond)); };
	}
	void HandleCommand(string command, Action<string> respond)
	{
		Log.Print($"[ProgramRoot] 处理命令: {command}");
		try
		{
			if (command.StartsWith("debug_get_node_details:"))
			{
				var nodePath = command["debug_get_node_details:".Length..];
				var nodeDetails = GetNodeDetails(nodePath);
				respond(nodeDetails);
				return;
			}
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
	string GetNodeDetails(string nodePath)
	{
		if (!nodePath.StartsWith("/")) nodePath = $"/root/{nodePath}";
		var node = GetNodeOrNull(nodePath);
		if (node == null) return $"错误: 找不到节点 '{nodePath}'";
		var nodeData = GD.VarToStr(node);
		return nodeData;
	}
}
