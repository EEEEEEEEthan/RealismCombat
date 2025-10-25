using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.Commands;
using RealismCombat.StateMachine;
namespace RealismCombat;
public partial class GameRoot : Node, IStateOwner
{
	public class PreparerState(GameRoot root) : State(root)
	{
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments)
		{
			Command? command = name switch
			{
				ShutdownCommand.name => new ShutdownCommand(root),
				CheckStatusCommand.name => new CheckStatusCommand(root),
				StartCombatCommand.name => new StartCombatCommand(root),
				DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(gameRoot: root, arguments: arguments),
				_ => null,
			};
			command?.Execute();
		}
		private protected override IEnumerable<string> GetAvailableCommands()
		{
			yield return ShutdownCommand.name;
			yield return CheckStatusCommand.name;
			yield return StartCombatCommand.name;
			yield return DebugShowNodeTreeCommand.name;
		}
	}
	public class CombatState(GameRoot root) : State(root)
	{
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments)
		{
			Command? command = name switch
			{
				ShutdownCommand.name => new ShutdownCommand(root),
				CheckStatusCommand.name => new CheckStatusCommand(root),
				DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(gameRoot: root, arguments: arguments),
				_ => null,
			};
			command?.Execute();
		}
		private protected override IEnumerable<string> GetAvailableCommands()
		{
			yield return ShutdownCommand.name;
			yield return CheckStatusCommand.name;
			yield return DebugShowNodeTreeCommand.name;
		}
	}
	static readonly IReadOnlyDictionary<string, string> arguments;
	static GameRoot()
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
	public readonly McpHandler? mcpHandler;
	public BattlePrepareScene? battlePrepareScene;
	public Combat? combat;
	State state;
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	public State State
	{
		get => state;
		set
		{
			state = value;
			Log.Print($"游戏进入状态:{state}");
			Log.Print($"可用指令: {string.Join(separator: ", ", values: state.AvailableCommands)}");
			mcpHandler?.McpCheckPoint();
		}
	}
	GameRoot()
	{
		state = new PreparerState(this);
		if (arguments.TryGetValue(key: "port", value: out var portText))
			if (int.TryParse(s: portText, result: out var port))
			{
				Log.Print($"启动服务器，端口: {port}");
				mcpHandler = new(gameRoot: this, port: port);
				mcpHandler.OnClientConnected += OnClientConnected;
				mcpHandler.OnClientDisconnected += OnClientDisconnected;
				Log.Print($"服务器已启动，监听端口 {port}");
			}
			else
			{
				Log.PrintError($"端口号非法: {port}");
			}
		else
			GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
		battlePrepareScene = BattlePrepareScene.Create(this);
		AddChild(battlePrepareScene);
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		mcpHandler?.Update();
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
