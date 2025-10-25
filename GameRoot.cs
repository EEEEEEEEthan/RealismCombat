using Godot;
using System.Text;
namespace RealismCombat;
public partial class GameRoot : Node
{
	Server? server;
	bool hadClientConnected;
	double totalTime;
	int frameCount;
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
		totalTime += delta;
		frameCount++;
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
				return BuildStatusResponse();
			default:
				return "unknown";
		}
	}
	string BuildStatusResponse()
	{
		var sb = new StringBuilder();
		sb.AppendLine("游戏运行状态:");
		sb.AppendLine($"  运行时间: {totalTime:F2}秒");
		sb.AppendLine($"  总帧数: {frameCount}");
		sb.AppendLine($"  平均FPS: {(totalTime > 0 ? frameCount / totalTime : 0):F2}");
		sb.AppendLine($"  当前FPS: {Engine.GetFramesPerSecond()}");
		sb.AppendLine($"  客户端已连接: {hadClientConnected}");
		if (server is not null)
		{
			sb.AppendLine("  服务器状态: 运行中");
			sb.AppendLine($"  待处理请求: {(server.PendingRequest is not null ? "有" : "无")}");
		}
		else
		{
			sb.AppendLine("  服务器状态: 未启动");
		}
		return sb.ToString().TrimEnd();
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => hadClientConnected = true;
	void OnClientDisconnected()
	{
		if (hadClientConnected) GetTree().Quit();
	}
}
