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
	readonly ConcurrentQueue<McpCommand> commandQueue = new();
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
		AutoLoad.GameServer.OnConnected += () => { Log.Print("[CommandHandler] MCP客户端已连接"); };
		AutoLoad.GameServer.OnDisconnected += () => { Log.Print("[CommandHandler] MCP客户端已断开"); };
		AutoLoad.GameServer.OnCommandReceived += command => { commandQueue.Enqueue(command); };
	}
	void HandleCommand(McpCommand cmd)
	{
		Log.Print($"[CommandHandler] 处理命令: {cmd.Command}");
		try
		{
			switch (cmd.Command)
			{
				case "system_launch_program":
					Log.Print("游戏已启动并连接成功");
					AutoLoad.GameServer.SendResponse();
					break;
				case "system_shutdown":
					Log.Print("正在关闭游戏");
					AutoLoad.GameServer.SendResponse();
					GetTree().CallDeferred(SceneTree.MethodName.Quit);
					break;
				case "debug_get_scene_tree":
					var treeJson = GetSceneTreeJson();
					Log.Print(treeJson);
					AutoLoad.GameServer.SendResponse();
					break;
				case "debug_get_node_details":
					if (cmd.TryGetArg("nodePath", out var nodePath))
					{
						var nodeDetails = GetNodeDetails(nodePath);
						Log.Print(nodeDetails);
					}
					else
					{
						Log.PrintErr("缺少参数: nodePath");
					}
					AutoLoad.GameServer.SendResponse();
					break;
				case "ping":
					Log.Print("pong");
					AutoLoad.GameServer.SendResponse();
					break;
				default:
					Log.PrintErr($"未知命令: {cmd.Command}");
					AutoLoad.GameServer.SendResponse();
					break;
			}
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
			AutoLoad.GameServer.SendResponse();
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
