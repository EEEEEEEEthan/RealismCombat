using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
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
		public IdleState(ProgramRootNode programRootNode) : base(programRootNode)
		{
			programRootNode.state = this;
			var dialogue = programRootNode.CreateDialogue();
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
							programRootNode.state = new GameState(programRootNode);
							dialogue.QueueFree();
							var gameNode = GameNode.Create(new());
							programRootNode.AddChild(gameNode);
							Log.Print("开始游戏,但是功能还没做");
							programRootNode.McpRespond();
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
							Log.Print("游戏即将退出...");
							programRootNode.McpRespond();
							programRootNode.GetTree().Quit();
						},
						available = true,
					},
				],
			});
		}
	}
	public class GameState : State
	{
		public GameState(ProgramRootNode programRootNode) : base(programRootNode) => programRootNode.state = this;
	}
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
	GenericDialogue? currentPopMessage;
	public bool HadClientConnected { get; private set; }
	AudioStreamPlayer bgmPlayer = null!;
	AudioStreamPlayer soundEffectPlayer = null!;
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
					if (currentPopMessage != null && currentPopMessage.autoContinue <= 0)
						currentPopMessage.autoContinue = 1.5f;
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
		for (var i = count; i-- > 0;)
		{
			var node = dialogues.GetChild(i);
			if (node is MenuDialogue dialogue) dialogue.Active = i == count - 1;
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
		soundEffectPlayer = GetNode<AudioStreamPlayer>("SoundEffectPlayer");
		state = new IdleState(this);
	}
	public void PlayMusic(string audioPath)
	{
		var audioStream = GD.Load<AudioStream>(audioPath);
		if (audioStream is AudioStreamMP3 mp3Stream)
		{
			mp3Stream.Loop = true;
		}
		bgmPlayer.Stream = audioStream;
		bgmPlayer.Play();
		if (audioPath == AudioTable.wizardattack26801)
		{
			bgmPlayer.Seek(2.0f);
		}
	}
	public void PlaySoundEffect(string audioPath)
	{
		var audioStream = GD.Load<AudioStream>(audioPath);
		soundEffectPlayer.Stream = audioStream;
		soundEffectPlayer.Play();
	}
}
