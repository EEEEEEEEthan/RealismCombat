// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer;
[McpServerToolType]
static class SystemTools
{
	const string LocalSettingsFileName = ".local.settings";
	const string GodotKey = "godot";
	const string DefaultGodotValue = "godot.exe";
	const string PortKey = "port";
	const string DefaultPortValue = "3000";
	/// <summary>
	///     向 Godot 发送 hello 请求测试短连接通信
	/// </summary>
	[McpServerTool, Description("hello"),]
	static async Task<string> hello()
	{
		var rootDir = FindProjectRoot();
		var settingsPath = Path.Combine(rootDir, LocalSettingsFileName);
		var settings = LoadOrCreateSettings(settingsPath);
		if (!settings.TryGetValue(PortKey, out var value))
		{
			AppendSettingIfMissing(settingsPath, PortKey, DefaultPortValue);
			value = DefaultPortValue;
			settings[PortKey] = value;
		}
		var port = int.Parse(value);
		return await SendShortRequest("127.0.0.1", port, "hello", 3000);
	}
	/// <summary>
	///     启动 Godot 运行当前项目。如果根目录缺少本机配置文件或缺少 godot 配置，将自动创建与补全默认值。
	/// </summary>
	[McpServerTool, Description("start game"),]
	static string start_game()
	{
		var rootDir = FindProjectRoot();
		var settingsPath = Path.Combine(rootDir, LocalSettingsFileName);
		var settings = LoadOrCreateSettings(settingsPath);
		if (!settings.TryGetValue(GodotKey, out var configuredGodot))
		{
			AppendSettingIfMissing(settingsPath, GodotKey, DefaultGodotValue);
			configuredGodot = DefaultGodotValue;
			settings[GodotKey] = configuredGodot;
		}
		if (!settings.TryGetValue(PortKey, out var value))
		{
			AppendSettingIfMissing(settingsPath, PortKey, DefaultPortValue);
			value = DefaultPortValue;
			settings[PortKey] = value;
		}
		var fileNameToStart = ResolveGodotExecutable(rootDir, configuredGodot);
		var port = int.Parse(value);
		try
		{
			var psi = new ProcessStartInfo
			{
				FileName = fileNameToStart,
				WorkingDirectory = rootDir,
				UseShellExecute = true,
				Arguments = $"--path . --port={port}",
			};
			var proc = Process.Start(psi);
			if (proc == null) return $"启动失败: 未能创建进程。可编辑 {LocalSettingsFileName} 配置 godot 路径。";
			return $"游戏启动成功！PID={proc.Id}，端口={port}";
		}
		catch (Exception ex)
		{
			return $"启动失败: {ex.Message}。请检查根目录下 {LocalSettingsFileName} 的 godot 配置。";
		}
	}
	/// <summary>
	///     查找项目根目录，优先向上搜索包含 project.godot 的目录。
	/// </summary>
	static string FindProjectRoot()
	{
		var dir = AppContext.BaseDirectory;
		try
		{
			var current = new DirectoryInfo(dir);
			while (current != null)
			{
				var marker = Path.Combine(current.FullName, "project.godot");
				if (File.Exists(marker)) return current.FullName;
				current = current.Parent;
			}
		}
		catch
		{
			// 忽略目录遍历异常
		}
		return dir;
	}
	/// <summary>
	///     读取或创建本机配置文件，采用一行一个的 AAA = BBB 格式。
	/// </summary>
	static Dictionary<string, string> LoadOrCreateSettings(string settingsPath)
	{
		if (!File.Exists(settingsPath))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(settingsPath) ?? ".");
			File.WriteAllText(settingsPath, $"{GodotKey} = {DefaultGodotValue}{Environment.NewLine}", new UTF8Encoding(false));
			return new(StringComparer.OrdinalIgnoreCase)
			{
				{ GodotKey, DefaultGodotValue },
			};
		}
		var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (var line in File.ReadAllLines(settingsPath, Encoding.UTF8))
		{
			var trimmed = line.Trim();
			if (string.IsNullOrEmpty(trimmed)) continue;
			var eqIndex = trimmed.IndexOf('=');
			if (eqIndex < 0) continue;
			var key = trimmed.Substring(0, eqIndex).Trim();
			var value = trimmed.Substring(eqIndex + 1).Trim();
			if (key.Length == 0) continue;
			dict[key] = value;
		}
		return dict;
	}
	/// <summary>
	///     如果缺失键则在配置文件末尾追加 AAA = BBB 行。
	/// </summary>
	static void AppendSettingIfMissing(string settingsPath, string key, string value) =>
		File.AppendAllText(settingsPath, $"{key} = {value}{Environment.NewLine}", new UTF8Encoding(false));
	/// <summary>
	///     根据配置值解析可执行路径。支持将目录作为配置，自动选择目录下最可能的 Godot 可执行文件。
	///     优先考虑根目录相对路径，其次使用原值交给系统 PATH 解析。
	/// </summary>
	static string ResolveGodotExecutable(string rootDir, string configured)
	{
		if (string.IsNullOrWhiteSpace(configured)) return DefaultGodotValue;

		// 绝对路径：可能是文件或目录
		if (Path.IsPathRooted(configured))
		{
			if (Directory.Exists(configured)) return PreferGodotExeInDirectory(configured);
			return configured;
		}

		// 相对路径：映射到项目根目录
		var combined = Path.Combine(rootDir, configured);
		if (Directory.Exists(combined)) return PreferGodotExeInDirectory(combined);
		if (File.Exists(combined)) return combined;
		return configured;
	}
	/// <summary>
	///     在指定目录内挑选最合适的 Godot 可执行文件。
	///     优先包含 "godot"，其次偏好包含 "mono"、"win64" 的文件名。
	///     若未找到，则尝试目录下的 godot.exe；再不行则返回该目录中任意一个 .exe 或回退为 目录\godot.exe。
	/// </summary>
	static string PreferGodotExeInDirectory(string directoryPath)
	{
		try
		{
			var exeFiles = Directory.GetFiles(directoryPath, "*.exe");
			var bestPath = string.Empty;
			var bestScore = -1;
			for (var i = 0; i < exeFiles.Length; i++)
			{
				var file = exeFiles[i];
				var nameLower = Path.GetFileName(file).ToLowerInvariant();
				var score = 0;
				if (nameLower.Contains("godot")) score += 10;
				if (nameLower.Contains("mono")) score += 5;
				if (nameLower.Contains("win64")) score += 1;
				if (score > bestScore)
				{
					bestScore = score;
					bestPath = file;
				}
			}
			if (bestScore >= 10) return bestPath; // 找到包含 godot 的可执行
			var defaultExe = Path.Combine(directoryPath, "godot.exe");
			if (File.Exists(defaultExe)) return defaultExe;
			if (!string.IsNullOrEmpty(bestPath)) return bestPath;
		}
		catch
		{
			// 忽略目录读取异常
		}
		return Path.Combine(directoryPath, "godot.exe");
	}
	/// <summary>
	///     发送短连接请求到 Godot 服务器
	/// </summary>
	static async Task<string> SendShortRequest(string host, int port, string request, int timeoutMs)
	{
		TcpClient? client = null;
		try
		{
			client = new();
			var connectTask = client.ConnectAsync(host, port);
			var timeoutTask = Task.Delay(timeoutMs);
			var completedTask = await Task.WhenAny(connectTask, timeoutTask);
			if (completedTask == timeoutTask || !client.Connected) return "连接超时";
			var stream = client.GetStream();
			stream.ReadTimeout = timeoutMs;
			stream.WriteTimeout = timeoutMs;
			var requestBytes = Encoding.UTF8.GetBytes(request);
			await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
			var buffer = new byte[1024];
			var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
			var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
			return response;
		}
		catch (Exception ex)
		{
			return $"请求失败: {ex.Message}";
		}
		finally
		{
			client?.Close();
		}
	}
}
