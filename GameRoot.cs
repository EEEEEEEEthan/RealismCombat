using Godot;
namespace RealismCombat;
public partial class GameRoot : Node
{
	CommandHandler? commandHandler;
	public Server? Server { get; private set; }
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	public override void _Ready()
	{
		commandHandler = new(this);
		var args = OS.GetCmdlineUserArgs();
		foreach (var a in args)
			if (a.StartsWith("--port="))
			{
				var value = a.Substring("--port=".Length);
				if (int.TryParse(s: value, result: out var port) && port > 0 && port < 65536)
				{
					GD.Print($"[GameRoot] 启动服务器，端口: {port}");
					Server = new(port);
					Server.OnClientConnected += OnClientConnected;
					Server.OnClientDisconnected += OnClientDisconnected;
					GD.Print($"[GameRoot] 服务器已启动，监听端口 {port}");
				}
				break;
			}
		if (Server is null) GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		if (Server is null || commandHandler is null) return;
		var cmd = Server.PendingRequest;
		if (cmd is null) return;
		var response = commandHandler.HandleCommand(cmd);
		Server.Respond(response);
	}
	public void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
