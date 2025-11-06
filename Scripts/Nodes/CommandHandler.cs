using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Godot;
namespace RealismCombat.Nodes;
/// <summary>
///     命令处理器，负责处理来自MCP客户端的指令
/// </summary>
public partial class CommandHandler : Node
{
	readonly ConcurrentQueue<(string command, Action<string> respond)> commandQueue = new();
	readonly ProgramRootNode programRoot;
	public CommandHandler(ProgramRootNode programRoot) => this.programRoot = programRoot;
	public override void _Ready() => Log.Print("[CommandHandler] 命令处理器已就绪");
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var item)) HandleCommand(item.command, item.respond);
	}
	public void SetupServerCallbacks()
	{
		var server = GetNode<AutoLoad.GameServer>("/root/GameServer");
		server.OnConnected += () => { Log.Print("[CommandHandler] MCP客户端已连接"); };
		server.OnDisconnected += () => { Log.Print("[CommandHandler] MCP客户端已断开"); };
		server.OnCommandReceived += (command, respond) => { commandQueue.Enqueue((command, respond)); };
	}
	void HandleCommand(string command, Action<string> respond)
	{
		Log.Print($"[CommandHandler] 处理命令: {command}");
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
					Log.PrintErr($"[CommandHandler] 未知命令: {command}");
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
