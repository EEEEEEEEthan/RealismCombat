using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Godot;
/// <summary>
///     命令处理器，负责处理来自MCP客户端的指令
/// </summary>
public partial class CommandHandler(ProgramRoot programRoot) : Node
{
	readonly ConcurrentQueue<McpCommand> commandQueue = new();
	public override void _EnterTree()
	{
		base._EnterTree();
		Name = nameof(CommandHandler);
	}
	public override void _Ready()
	{
		Log.Print("[CommandHandler] 命令处理器已就绪");
		SetupServerCallbacks();
	}
	public override void _Process(double delta)
	{
		while (commandQueue.TryDequeue(out var command))
			try
			{
				HandleCommand(command);
			}
			catch (Exception e)
			{
				Log.PrintException(e);
				GameServer.McpCheckpoint();
			}
	}
	void SetupServerCallbacks()
	{
		GameServer.OnConnected += () => { Log.Print("[CommandHandler] MCP客户端已连接"); };
		GameServer.OnDisconnected += () => { Log.Print("[CommandHandler] MCP客户端已断开"); };
		GameServer.OnCommandReceived += command => { commandQueue.Enqueue(command); };
	}
	void HandleCommand(McpCommand cmd)
	{
		Log.Print($"[CommandHandler] 处理命令: {cmd.Command}");
		try
		{
			switch (cmd.Command)
			{
				case "system_launch_program":
					programRoot.StartGameLoop();
					break;
				case "debug_get_scene_tree":
					var treeJson = GetSceneTreeJson();
					Log.Print(treeJson);
					GameServer.McpCheckpoint();
					break;
				case "debug_get_node_details":
				{
					Log.Print(GetNodeDetails(cmd.Args["nodePath"]));
					GameServer.McpCheckpoint();
					break;
				}
				case "game_select_option":
				{
					var index = int.Parse(cmd.Args["id"]);
					((MenuDialogue)DialogueManager.GetTopDialogue()!).SelectAndConfirm(index);
					break;
				}
				default:
					Log.PrintError($"未知命令: {cmd.Command}");
					GameServer.McpCheckpoint();
					break;
			}
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
			GameServer.McpCheckpoint();
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
