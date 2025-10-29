using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
partial class ProgramRootNode : Node
{
	static readonly IReadOnlyDictionary<string, string> arguments;
	static ProgramRootNode()
	{
		var dict = new Dictionary<string, string>();
		var regex = new Regex(@"--(\S+)=(\S+)");
		foreach (var arg in OS.GetCmdlineUserArgs())
		{
			var match = regex.Match(arg);
			if (match.Success) dict[match.Groups[1].ToString()] = match.Groups[2].ToString();
		}
		arguments = dict;
	}
	readonly McpHandler? mcpHandler;
	[Export] Container dialogues = null!;
	public bool HadClientConnected { get; private set; }
	ProgramRootNode()
	{
		if (arguments.TryGetValue(key: "port", value: out var portText))
			if (int.TryParse(s: portText, result: out var port))
			{
				Log.Print($"启动服务器，端口: {port}");
				mcpHandler = new(programRootNode: this, port: port);
				mcpHandler.OnClientConnected += onClientConnected;
				mcpHandler.OnClientDisconnected += onClientDisconnected;
				Log.Print($"服务器已启动，监听端口 {port}");
				void onClientConnected() => HadClientConnected = true;
				void onClientDisconnected()
				{
					if (HadClientConnected) GetTree().Quit();
				}
			}
			else
			{
				Log.PrintError($"端口号非法: {port}");
			}
		else
			GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public void McpRespond() => mcpHandler?.McpRespond();
	public void OnMcpRequest(string command) { }
	public override void _Process(double delta)
	{
		mcpHandler?.Update();
		if (dialogues.GetChildCount() > 1) dialogues.GetChild<MenuDialogue>(dialogues.GetChildCount() - 1).Active = true;
	}
	public MenuDialogue CreateDialogue()
	{
		var dialogue = MenuDialogue.Create(this);
		if (dialogues.GetChildCount() > 1) dialogues.GetChild<MenuDialogue>(dialogues.GetChildCount() - 1).Active = false;
		dialogues.AddChild(dialogue);
		return dialogue;
	}
	public override void _Ready()
	{
		var dialogue = MenuDialogue.Create(this);
		if (dialogues.GetChildCount() > 1) dialogue.GetChild<MenuDialogue>(dialogues.GetChildCount() - 1).Active = false;
		dialogues.AddChild(dialogue);
		dialogue.Initialize(new()
		{
			title = "主菜单",
			options =
			[
				new()
				{
					option = "开始游戏",
					description = "开始新游戏",
					onPreview = () => { },
					onConfirm = () =>
					{
						dialogue.QueueFree();
						McpRespond();
					},
					available = true,
				},
				new()
				{
					option = "退出",
					description = "退出游戏",
					onPreview = () => { },
					onConfirm = () =>
					{
						McpRespond();
						GetTree().Quit();
					},
					available = true,
				},
			],
		});
	}
}
