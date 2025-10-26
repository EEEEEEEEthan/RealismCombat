using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.Commands;
using RealismCombat.StateMachine;
namespace RealismCombat;
public partial class GameRoot : Node, IStateOwner
{
	public class PreparerState : State
	{
		public readonly GameRoot gameRoot;
		public override string Status =>
			$"""
			准备阶段
			可用指令: {CheckStatusCommand.name}, {StartCombatCommand.name}, {ShutdownCommand.name}, {DebugShowNodeTreeCommand.name}
			""";
		public PreparerState(GameRoot gameRoot) : base(gameRoot)
		{
			this.gameRoot = gameRoot;
			Log.Print("准备中");
			gameRoot.mcpHandler?.McpCheckPoint();
		}
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments)
		{
			Command command = name switch
			{
				ShutdownCommand.name => new ShutdownCommand(gameRoot),
				CheckStatusCommand.name => new CheckStatusCommand(gameRoot),
				StartCombatCommand.name => new StartCombatCommand(gameRoot),
				DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(gameRoot: gameRoot, arguments: arguments),
				_ => throw new ArgumentException($"当前状态无法执行{name}"),
			};
			command.Execute();
		}
	}
	public class CombatState(GameRoot root) : State(root)
	{
		public override string Status =>
			$"""
		战斗阶段
		可用指令: {CheckStatusCommand.name}, {ShutdownCommand.name}, {DebugShowNodeTreeCommand.name}
		""";
		protected override void ExecuteCommand(string name, IReadOnlyDictionary<string, string> arguments)
		{
			Command command = name switch
			{
				ShutdownCommand.name => new ShutdownCommand(root),
				CheckStatusCommand.name => new CheckStatusCommand(root),
				DebugShowNodeTreeCommand.name => new DebugShowNodeTreeCommand(gameRoot: root, arguments: arguments),
				_ => throw new ArgumentException($"当前状态无法执行{name}"),
			};
			command.Execute();
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
	public BattlePrepareScene? battlePrepareScene;
	public Combat? combat;
	readonly McpHandler? mcpHandler;
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	public State State { get; private set; }
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	GameRoot()
	{
		State = new PreparerState(this);
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
	public void McpCheckPoint() => mcpHandler?.McpCheckPoint();
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
