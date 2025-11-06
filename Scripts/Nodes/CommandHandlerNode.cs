using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Godot;
namespace RealismCombat.Nodes;
/// <summary>
///     命令处理器，负责处理来自MCP客户端的指令
/// </summary>
public partial class CommandHandlerNode(ProgramRootNode programRoot) : Node
{
	readonly ConcurrentQueue<string> commandQueue = new();
	AutoLoad.GameServer? server;
	public override void _EnterTree()
	{
		base._EnterTree();
		Name = nameof(CommandHandlerNode);
	}
	public override void _Ready()
	{
		Log.Print("[CommandHandler] 命令处理器已就绪");
		SetupServerCallbacks();
	}
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var command)) HandleCommand(command);
	}
	void SetupServerCallbacks()
	{
		server = GetNode<AutoLoad.GameServer>("/root/GameServer");
		server.OnConnected += () => { Log.Print("[CommandHandler] MCP客户端已连接"); };
		server.OnDisconnected += () => { Log.Print("[CommandHandler] MCP客户端已断开"); };
		server.OnCommandReceived += command => { commandQueue.Enqueue(command); };
	}
	void HandleCommand(string command)
	{
		Log.Print($"[CommandHandler] 处理命令: {command}");
		if (server == null)
		{
			Log.PrintErr("[CommandHandler] GameServer未初始化");
			return;
		}
		try
		{
			if (command.StartsWith("debug_get_node_details:"))
			{
				var nodePath = command["debug_get_node_details:".Length..];
				var nodeDetails = GetNodeDetails(nodePath);
				Log.Print(nodeDetails);
				server.SendResponse();
				return;
			}
			switch (command)
			{
				case "system_launch_program":
					Log.Print("游戏已启动并连接成功");
					server.SendResponse();
					break;
				case "system_shutdown":
					Log.Print("正在关闭游戏");
					server.SendResponse();
					GetTree().CallDeferred(SceneTree.MethodName.Quit);
					break;
				case "debug_get_scene_tree":
					var treeJson = GetSceneTreeJson();
					Log.Print(treeJson);
					server.SendResponse();
					break;
				case "ping":
					Log.Print("pong");
					server.SendResponse();
					break;
				default:
					Log.PrintErr($"未知命令: {command}");
					server.SendResponse();
					break;
			}
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
			server.SendResponse();
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
