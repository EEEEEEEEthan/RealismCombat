using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.Extensions;
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
	MenuDialogue? dialogue;
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
	public override void _Process(double delta) { }
	public void McpRequest(string command) { }
	public override void _Ready()
	{
		var dialogue = CreateDialogue();
		dialogue.Show(new DialogueData
		{
			title = "主菜单",
			options = new[]
			{
				new DialogueOptionData
				{
					option = "开始游戏",
					description = "开始新游戏",
					onPreview = () => { },
					onConfirm = () => { },
					available = true
				},
				new DialogueOptionData
				{
					option = "退出",
					description = "退出游戏",
					onPreview = () => { },
					onConfirm = _QuitGame,
					available = true
				}
			}
		});
	}
	public MenuDialogue CreateDialogue()
	{
		if (dialogue?.Valid() == true) throw new InvalidOperationException("当前没有活动对话框，无法设置标题");
		dialogue = MenuDialogue.Create();
		return dialogue;
	}
	void _QuitGame() => GetTree().Quit();
}
