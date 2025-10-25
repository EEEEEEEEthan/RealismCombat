using Godot;
namespace RealismCombat;
public partial class GameRoot : Node
{
	public readonly CommandHandler commandHandler;
	public McpHandler? McpHandler { get; private set; }
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	GameRoot() => commandHandler = new(this);
	public override void _Ready()
	{
		var args = OS.GetCmdlineUserArgs();
		foreach (var a in args)
			if (a.StartsWith("--port="))
			{
				var value = a["--port=".Length..];
				if (ushort.TryParse(s: value, result: out var port))
				{
					GD.Print($"[GameRoot] 启动服务器，端口: {port}");
					McpHandler = new(gameRoot: this, port: port);
					McpHandler.OnClientConnected += OnClientConnected;
					McpHandler.OnClientDisconnected += OnClientDisconnected;
					GD.Print($"[GameRoot] 服务器已启动，监听端口 {port}");
				}
				break;
			}
		if (McpHandler is null) GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		McpHandler?.Update();
	}
	public void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
