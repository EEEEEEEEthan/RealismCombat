using Godot;
namespace RealismCombat;
/// <summary>
///     启动参数解析器，将命令行参数解析并存储为静态字段
/// </summary>
public static class LaunchArgs
{
	public static readonly int? port;
	static LaunchArgs()
	{
		var args = OS.GetCmdlineUserArgs();
		Log.Print($"[LaunchArgs] 用户命令行参数: {string.Join(", ", args)}");
		for (var i = 0; i < args.Length; i++)
		{
			var arg = args[i];
			if (arg.StartsWith("--port="))
			{
				var portStr = arg["--port=".Length..];
				if (int.TryParse(portStr, out var parsedPort))
				{
					port = parsedPort;
					Log.Print($"[LaunchArgs] 从命令行参数获取端口: {port}");
				}
				else
				{
					Log.PrintErr($"[LaunchArgs] 无效的端口参数: {portStr}");
				}
			}
		}
		if (!port.HasValue) Log.Print("[LaunchArgs] 未指定端口，以普通模式运行");
	}
}
