using Godot;
namespace RealismCombat;
public partial class GameRoot : Node
{
	Server? server;
	bool hadClientConnected;
	public override void _Ready()
	{
		var args = OS.GetCmdlineUserArgs();
		foreach (var a in args)
			if (a.StartsWith("--port="))
			{
				var value = a.Substring("--port=".Length);
				if (int.TryParse(s: value, result: out var port) && port > 0 && port < 65536)
				{
					GD.Print($"[GameRoot] 启动服务器，端口: {port}");
					server = new(port);
					server.OnClientConnected += OnClientConnected;
					server.OnClientDisconnected += OnClientDisconnected;
					GD.Print($"[GameRoot] 服务器已启动，监听端口 {port}");
				}
				break;
			}
		if (server is null) GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public override void _Process(double delta)
	{
		if (server is null) return;
		var cmd = server.PendingRequest;
		if (cmd is null) return;
		var response = HandleCommand(cmd);
		server.Respond(response);
	}
	string HandleCommand(string cmd)
	{
		switch (cmd)
		{
			case "system.shutdown":
				CallDeferred(MethodName._QuitGame);
				return "ok";
			case "game.check_status":
				return "ok";
			default:
				return "unknown";
		}
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => hadClientConnected = true;
	void OnClientDisconnected()
	{
		if (hadClientConnected) GetTree().Quit();
	}
}
