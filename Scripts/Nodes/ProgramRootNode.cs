using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.Data;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
public partial class ProgramRootNode : Node
{
	public abstract class State(ProgramRootNode programRootNode)
	{
		public readonly ProgramRootNode programRootNode = programRootNode;
	}
	public class IdleState : State
	{
		static void ShowSettingsMenu(ProgramRootNode programRootNode)
		{
			var settings = Settings.Load();
			var settingsDialogue = programRootNode.CreateDialogue();
			var options = new List<DialogueOptionData>
			{
				new()
				{
					option = "[dev]跳过存档",
					description = settings.SkipSave ? "当前: 是" : "当前: 否",
					onPreview = () => { },
					onConfirm = () => { ShowSkipSaveMenu(programRootNode: programRootNode, settings: settings, settingsDialogue: settingsDialogue); },
					available = true,
				},
			};
			settingsDialogue.Initialize(data: new()
				{
					title = "设置",
					options = options,
				},
				onReturn: () => { },
				returnDescription: "返回主菜单");
		}
		static void ShowSkipSaveMenu(ProgramRootNode programRootNode, Settings settings, MenuDialogue settingsDialogue)
		{
			var skipSaveDialogue = programRootNode.CreateDialogue();
			var options = new List<DialogueOptionData>
			{
				new()
				{
					option = "是",
					description = "启用跳过存档",
					onPreview = () => { },
					onConfirm = () =>
					{
						settings.SkipSave = true;
						settings.Save();
						Log.Print("已启用跳过存档");
						if (IsInstanceValid(skipSaveDialogue) && skipSaveDialogue.IsInsideTree()) skipSaveDialogue.QueueFree();
						if (IsInstanceValid(settingsDialogue) && settingsDialogue.IsInsideTree()) settingsDialogue.QueueFree();
					},
					available = true,
				},
				new()
				{
					option = "否",
					description = "禁用跳过存档",
					onPreview = () => { },
					onConfirm = () =>
					{
						settings.SkipSave = false;
						settings.Save();
						Log.Print("已禁用跳过存档");
						if (IsInstanceValid(skipSaveDialogue) && skipSaveDialogue.IsInsideTree()) skipSaveDialogue.QueueFree();
						if (IsInstanceValid(settingsDialogue) && settingsDialogue.IsInsideTree()) settingsDialogue.QueueFree();
					},
					available = true,
				},
			};
			skipSaveDialogue.Initialize(data: new()
				{
					title = "跳过存档",
					options = options,
				},
				onReturn: () => { },
				returnDescription: "返回");
		}
		public IdleState(ProgramRootNode programRootNode) : base(programRootNode)
		{
			programRootNode.state = this;
			var dialogue = programRootNode.CreateDialogue();
			var options = new List<DialogueOptionData>
			{
				new()
				{
					option = "开始游戏",
					description = "开始新游戏",
					onPreview = () => { },
					onConfirm = () =>
					{
						programRootNode.state = new GameState(programRootNode);
						dialogue.QueueFree();
						var gameNode = GameNode.Create(new());
						programRootNode.AddChild(gameNode);
						programRootNode.McpRespond();
					},
					available = true,
				},
			};
			if (File.Exists(Persistant.saveDataPath))
				options.Add(new()
				{
					option = "读档",
					description = "读取存档",
					onPreview = () => { },
					onConfirm = () =>
					{
						try
						{
							var gameData = Persistant.Load(Persistant.saveDataPath);
							programRootNode.state = new GameState(programRootNode);
							dialogue.QueueFree();
							var gameNode = GameNode.Create(gameData);
							programRootNode.AddChild(gameNode);
							Log.Print("已读取存档");
						}
						catch (Exception)
						{
							programRootNode.PopMessage("读档失败");
							programRootNode.McpRespond();
							throw;
						}
					},
					available = true,
				});
			options.Add(new()
			{
				option = "设置",
				description = "游戏设置",
				onPreview = () => { },
				onConfirm = () => { ShowSettingsMenu(programRootNode: programRootNode); },
				available = true,
			});
			options.Add(new()
			{
				option = "退出",
				description = "退出游戏",
				onPreview = () => { },
				onConfirm = () =>
				{
					Log.Print("游戏即将退出...");
					programRootNode.McpRespond();
					programRootNode.GetTree().Quit();
				},
				available = true,
			});
			dialogue.Initialize(new()
			{
				title = "主菜单",
				options = options,
			});
		}
	}
	public class GameState : State
	{
		public GameState(ProgramRootNode programRootNode) : base(programRootNode) => programRootNode.state = this;
	}
	const int MaxSoundEffectPlayers = 16;
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
	static string BuildNodeTree(Node root)
	{
		var sb = new StringBuilder();
		BuildNodeTreeRecursive(node: root, sb: sb, depth: 0);
		return sb.ToString();
	}
	static void BuildNodeTreeRecursive(Node node, StringBuilder sb, int depth)
	{
		var indent = new string(c: ' ', count: depth * 4);
		var nodeName = node.Name;
		var nodeType = node.GetType().Name;
		var childCount = node.GetChildCount();
		if (childCount == 0)
		{
			sb.AppendLine($"{indent}\"{nodeName}({nodeType})\"");
		}
		else
		{
			sb.AppendLine($"{indent}\"{nodeName}({nodeType})\": {{");
			for (var i = 0; i < childCount; i++)
			{
				var child = node.GetChild(i);
				BuildNodeTreeRecursive(node: child, sb: sb, depth: depth + 1);
				if (i < childCount - 1)
				{
					sb.Remove(startIndex: sb.Length - 1, length: 1);
					sb.AppendLine(",");
				}
			}
			sb.AppendLine($"{indent}}}");
		}
	}
	public State state = null!;
	[Export] public Container dialogues = null!;
	readonly McpHandler? mcpHandler;
	readonly List<AudioStreamPlayer> soundEffectPlayers = [];
	GenericDialogue? currentPopMessage;
	AudioStreamPlayer bgmPlayer = null!;
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
				void onClientConnected()
				{
					HadClientConnected = true;
					// 如果当前有弹窗且未设置自动继续，在工具连接后启用自动继续
					if (currentPopMessage != null && currentPopMessage.autoContinue <= 0) currentPopMessage.autoContinue = 1.5f;
				}
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
			Log.Print("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public void McpRespond() => mcpHandler?.McpRespond();
	public void OnMcpRequest(string command)
	{
		// system_launch_program
		if (new Regex(@"system_launch_program").TryMatch(text: command, match: out _))
		{
			// 发送最新的dialogue信息
			var dialogue = dialogues.GetChild<MenuDialogue>(dialogues.GetChildCount() - 1);
			dialogue.Active = false;
			dialogue.Active = true;
		}
		if (new Regex(@"game_select_option (\d+)").TryMatch(text: command, match: out var match))
		{
			var optionId = int.Parse(match.Groups[1].ToString());
			if (dialogues.GetChildCount() == 0) throw new("No dialogues available to select option from.");
			var currentDialogue = dialogues.GetChild<MenuDialogue>(dialogues.GetChildCount() - 1);
			currentDialogue.Confirm(optionId);
		}
		else if (new Regex(@"show_node_tree").TryMatch(text: command, match: out _))
		{
			var tree = BuildNodeTree(this);
			Log.Print(tree);
			McpRespond();
		}
		else if (new Regex(@"game_quit").TryMatch(text: command, match: out _))
		{
			McpRespond();
			GetTree().Quit();
		}
	}
	public override void _Process(double delta)
	{
		mcpHandler?.Update();
		var count = dialogues.GetChildCount();
		for (var i = 0; i < count; i++)
		{
			var node = dialogues.GetChild(i);
			if (node is MenuDialogue dialogue)
			{
				dialogue.Active = i == count - 1;
				const float fullWidth = 16.0f;
				const float collapsedWidth = 0.0f;
				const int keepVisibleCount = 2;
				var visibleFromIndex = Math.Max(val1: 0, val2: count - keepVisibleCount);
				dialogue.SetTargetWidth(i >= visibleFromIndex ? fullWidth : collapsedWidth);
			}
		}
	}
	public MenuDialogue CreateDialogue()
	{
		var dialogue = MenuDialogue.Create(this);
		dialogues.AddChild(dialogue);
		return dialogue;
	}
	public GenericDialogue PopMessage(string message)
	{
		if (currentPopMessage != null)
		{
			currentPopMessage.QueueFree();
			currentPopMessage = null;
		}
		var dialogue = GenericDialogue.Create();
		dialogue.Text = message;
		// 仅当 MCP 工具已连接后才自动继续，否则需按键继续
		dialogue.autoContinue = HadClientConnected ? 1.5f : 0f;
		currentPopMessage = dialogue;
		dialogue.OnDestroy += () => currentPopMessage = null;
		AddChild(dialogue);
		return dialogue;
	}
	public override void _Ready()
	{
		bgmPlayer = GetNode<AudioStreamPlayer>("BgmPlayer");
		var initialSoundEffectPlayer = GetNode<AudioStreamPlayer>("SoundEffectPlayer");
		soundEffectPlayers.Add(initialSoundEffectPlayer);
		state = new IdleState(this);
	}
	public void PlayMusic(string audioPath)
	{
		var audioStream = GD.Load<AudioStream>(audioPath);
		if (audioStream is AudioStreamMP3 mp3Stream) mp3Stream.Loop = true;
		bgmPlayer.Stream = audioStream;
		bgmPlayer.Play();
	}
	public void PlaySoundEffect(string audioPath)
	{
		var audioStream = GD.Load<AudioStream>(audioPath);
		var player = GetAvailableSoundEffectPlayer();
		player.Stream = audioStream;
		player.Play();
	}
	AudioStreamPlayer GetAvailableSoundEffectPlayer()
	{
		foreach (var player in soundEffectPlayers)
			if (!player.Playing)
				return player;
		if (soundEffectPlayers.Count < MaxSoundEffectPlayers)
		{
			var newPlayer = new AudioStreamPlayer();
			AddChild(newPlayer);
			soundEffectPlayers.Add(newPlayer);
			return newPlayer;
		}
		return soundEffectPlayers[0];
	}
}
