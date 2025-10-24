using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace RealismCombat.McpServer;
public static class GameClient
{
	const string localSettingsFileName = ".local.settings";
	const string godotKey = "godot";
	const string defaultGodotValue = "godot.exe";
	const string portKey = "port";
	const string defaultPortValue = "3000";
	public static async Task<string> SendCommand(string command, int timeoutMs)
	{
		var rootDir = FindProjectRoot();
		var settingsPath = Path.Combine(rootDir, localSettingsFileName);
		var settings = LoadOrCreateSettings(settingsPath);
		if (!settings.TryGetValue(portKey, out var value))
		{
			AppendSettingIfMissing(settingsPath, portKey, defaultPortValue);
			value = defaultPortValue;
			settings[portKey] = value;
		}
		var port = int.Parse(value);
		return await SendShortRequest("127.0.0.1", port, command, timeoutMs);
	}
	public static string StartGame()
	{
		var rootDir = FindProjectRoot();
		var settingsPath = Path.Combine(rootDir, localSettingsFileName);
		var settings = LoadOrCreateSettings(settingsPath);
		if (!settings.TryGetValue(godotKey, out var configuredGodot))
		{
			AppendSettingIfMissing(settingsPath, godotKey, defaultGodotValue);
			configuredGodot = defaultGodotValue;
			settings[godotKey] = configuredGodot;
		}
		if (!settings.TryGetValue(portKey, out var value))
		{
			AppendSettingIfMissing(settingsPath, portKey, defaultPortValue);
			value = defaultPortValue;
			settings[portKey] = value;
		}
		var fileNameToStart = ResolveGodotExecutable(rootDir, configuredGodot);
		var port = int.Parse(value);
		if (IsPortInUse(port)) return $"启动失败: 端口 {port} 已被占用。请修改 {localSettingsFileName} 中的端口配置或释放该端口。";
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
			if (proc == null) return $"启动失败: 未能创建进程。可编辑 {localSettingsFileName} 配置 godot 路径。";
			return $"游戏启动成功！PID={proc.Id}，端口={port}";
		}
		catch (Exception ex)
		{
			return $"启动失败: {ex.Message}。请检查根目录下 {localSettingsFileName} 的 godot 配置。";
		}
	}
	static bool IsPortInUse(int port)
	{
		try
		{
			var listener = new TcpListener(IPAddress.Any, port);
			listener.Start();
			listener.Stop();
			return false;
		}
		catch
		{
			return true;
		}
	}
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
			// ignored
		}
		return dir;
	}
	static Dictionary<string, string> LoadOrCreateSettings(string settingsPath)
	{
		if (!File.Exists(settingsPath))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(settingsPath) ?? ".");
			File.WriteAllText(settingsPath, $"{godotKey} = {defaultGodotValue}{Environment.NewLine}", new UTF8Encoding(false));
			return new(StringComparer.OrdinalIgnoreCase)
			{
				{ godotKey, defaultGodotValue },
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
	static void AppendSettingIfMissing(string settingsPath, string key, string value) =>
		File.AppendAllText(settingsPath, $"{key} = {value}{Environment.NewLine}", new UTF8Encoding(false));
	static string ResolveGodotExecutable(string rootDir, string configured)
	{
		if (string.IsNullOrWhiteSpace(configured)) return defaultGodotValue;
		if (Path.IsPathRooted(configured))
		{
			if (Directory.Exists(configured)) return PreferGodotExeInDirectory(configured);
			return configured;
		}
		var combined = Path.Combine(rootDir, configured);
		if (Directory.Exists(combined)) return PreferGodotExeInDirectory(combined);
		if (File.Exists(combined)) return combined;
		return configured;
	}
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
			if (bestScore >= 10) return bestPath;
			var defaultExe = Path.Combine(directoryPath, "godot.exe");
			if (File.Exists(defaultExe)) return defaultExe;
			if (!string.IsNullOrEmpty(bestPath)) return bestPath;
		}
		catch
		{
			// ignored
		}
		return Path.Combine(directoryPath, "godot.exe");
	}
	static async Task<string> SendShortRequest(string host, int port, string request, int timeoutMs)
	{
		TcpClient? client = null;
		try
		{
			client = new();
			var connect = client.ConnectAsync(host, port);
			var timeout = Task.Delay(timeoutMs);
			var completed = await Task.WhenAny(connect, timeout);
			if (completed == timeout || !client.Connected) return "连接超时";
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
