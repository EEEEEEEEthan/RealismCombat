using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using RealismCombat.StateMachine;
namespace RealismCombat.Nodes;
partial class ProgramRoot : Node, IStateOwner
{
	static readonly IReadOnlyDictionary<string, string> arguments;
	static ProgramRoot()
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
	ProgramRoot()
	{
		State = new MenuState(this);
		if (arguments.TryGetValue(key: "port", value: out var portText))
			if (int.TryParse(s: portText, result: out var port))
			{
				Log.Print($"启动服务器，端口: {port}");
				mcpHandler = new(programRoot: this, port: port);
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
	public void McpCheckPoint()
	{
		Log.Print(State.Status);
		mcpHandler?.McpCheckPoint();
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		mcpHandler?.Update();
		State.Update(delta);
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
