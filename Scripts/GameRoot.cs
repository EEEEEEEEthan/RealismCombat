using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
namespace RealismCombat;
public partial class GameRoot : Node
{
	public static readonly IReadOnlyDictionary<string, string> arguments;
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
	public readonly CommandHandler commandHandler;
	public McpHandler? McpHandler { get; private set; }
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	GameRoot() => commandHandler = new(this);
	public override void _Ready()
	{
		if (arguments.TryGetValue(key: "port", value: out var portText))
			if (ushort.TryParse(s: portText, result: out var port))
			{
				Log.Print($"启动服务器，端口: {port}");
				McpHandler = new(gameRoot: this, port: port);
				McpHandler.OnClientConnected += OnClientConnected;
				McpHandler.OnClientDisconnected += OnClientDisconnected;
				Log.Print($"服务器已启动，监听端口 {port}");
			}
			else
			{
				Log.PrintError($"端口号非法: {port}");
			}
		else
			GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		McpHandler?.Update();
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
