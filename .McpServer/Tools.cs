// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using ModelContextProtocol.Server;
namespace RealismCombat.McpServer
{
	[McpServerToolType]
	static class SystemTools
	{
		[McpServerTool, Description("hello"),] static Task<string> hello() => Task.FromResult("hello");

		/// <summary>
		/// 连接到指定端口的游戏实例并执行握手验证
		/// </summary>
		[McpServerTool, Description("connect to game instance on specified port"),]
		static async Task<string> connect_game(int port)
		{
			try
			{
				var result = await ConnectAndHandshake("127.0.0.1", port, 5000);
				return result 
					? $"成功连接到游戏实例，端口: {port}，握手完成" 
					: $"连接失败或握手超时，端口: {port}";
			}
			catch (Exception ex)
			{
				return $"连接游戏失败: {ex.Message}";
			}
		}

		/// <summary>
		/// 启动 Godot 运行当前项目。如果根目录缺少本机配置文件或缺少 godot 配置，将自动创建与补全默认值。
		/// </summary>
		[McpServerTool, Description("start game"),]
		static async Task<string> start_game()
		{
			var rootDir = FindProjectRoot();
			var settingsPath = Path.Combine(rootDir, LocalSettingsFileName);
			var settings = LoadOrCreateSettings(settingsPath);
			if (!settings.ContainsKey(GodotKey))
			{
				AppendSettingIfMissing(settingsPath, GodotKey, DefaultGodotValue);
				settings[GodotKey] = DefaultGodotValue;
			}

			var configuredGodot = settings[GodotKey];
			var fileNameToStart = ResolveGodotExecutable(rootDir, configuredGodot);

			var port = new Random().Next(9000, 10000);

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
				if (proc == null)
				{
					return $"启动失败: 未能创建进程。可编辑 {LocalSettingsFileName} 配置 godot 路径。";
				}

				await Task.Delay(2000);

				var handshakeSuccess = await ConnectAndHandshake("127.0.0.1", port, 5000);
				
				if (handshakeSuccess)
				{
					return $"游戏启动成功！PID={proc.Id}，端口={port}，握手完成";
				}
				else
				{
					return $"游戏已启动但握手失败。PID={proc.Id}，端口={port}，请检查游戏是否正常运行";
				}
			}
			catch (Exception ex)
			{
				return $"启动失败: {ex.Message}。请检查根目录下 {LocalSettingsFileName} 的 godot 配置。";
			}
		}

		/// <summary>
		/// 查找项目根目录，优先向上搜索包含 project.godot 的目录。
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
		/// 读取或创建本机配置文件，采用一行一个的 AAA = BBB 格式。
		/// </summary>
		static Dictionary<string, string> LoadOrCreateSettings(string settingsPath)
		{
			if (!File.Exists(settingsPath))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(settingsPath) ?? ".");
				File.WriteAllText(settingsPath, $"{GodotKey} = {DefaultGodotValue}{Environment.NewLine}", new UTF8Encoding(false));
				return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{ GodotKey, DefaultGodotValue }
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
		/// 如果缺失键则在配置文件末尾追加 AAA = BBB 行。
		/// </summary>
		static void AppendSettingIfMissing(string settingsPath, string key, string value)
		{
			File.AppendAllText(settingsPath, $"{key} = {value}{Environment.NewLine}", new UTF8Encoding(false));
		}

		/// <summary>
		/// 根据配置值解析可执行路径。支持将目录作为配置，自动选择目录下最可能的 Godot 可执行文件。
		/// 优先考虑根目录相对路径，其次使用原值交给系统 PATH 解析。
		/// </summary>
		static string ResolveGodotExecutable(string rootDir, string configured)
		{
			if (string.IsNullOrWhiteSpace(configured)) return DefaultGodotValue;

			// 绝对路径：可能是文件或目录
			if (Path.IsPathRooted(configured))
			{
				if (Directory.Exists(configured))
				{
					return PreferGodotExeInDirectory(configured);
				}
				return configured;
			}

			// 相对路径：映射到项目根目录
			var combined = Path.Combine(rootDir, configured);
			if (Directory.Exists(combined))
			{
				return PreferGodotExeInDirectory(combined);
			}
			if (File.Exists(combined))
			{
				return combined;
			}
			return configured;
		}

		/// <summary>
		/// 在指定目录内挑选最合适的 Godot 可执行文件。
		/// 优先包含 "godot"，其次偏好包含 "mono"、"win64" 的文件名。
		/// 若未找到，则尝试目录下的 godot.exe；再不行则返回该目录中任意一个 .exe 或回退为 目录\godot.exe。
		/// </summary>
		static string PreferGodotExeInDirectory(string directoryPath)
		{
			try
			{
				var exeFiles = Directory.GetFiles(directoryPath, "*.exe");
				string bestPath = string.Empty;
				int bestScore = -1;
				for (int i = 0; i < exeFiles.Length; i++)
				{
					var file = exeFiles[i];
					var nameLower = Path.GetFileName(file).ToLowerInvariant();
					int score = 0;
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
		/// 连接到指定地址和端口，执行 hello/world 握手
		/// </summary>
		static async Task<bool> ConnectAndHandshake(string host, int port, int timeoutMs)
		{
			TcpClient client = null;
			try
			{
				client = new TcpClient();
				var connectTask = client.ConnectAsync(host, port);
				var timeoutTask = Task.Delay(timeoutMs);
				
				var completedTask = await Task.WhenAny(connectTask, timeoutTask);
				if (completedTask == timeoutTask || !client.Connected)
				{
					return false;
				}

				var stream = client.GetStream();
				stream.ReadTimeout = timeoutMs;
				stream.WriteTimeout = timeoutMs;

				var helloBytes = Encoding.UTF8.GetBytes("hello\n");
				await stream.WriteAsync(helloBytes, 0, helloBytes.Length);

				var buffer = new byte[1024];
				var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
				var response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

				return response == "world";
			}
			catch
			{
				return false;
			}
			finally
			{
				client?.Close();
			}
		}

		const string LocalSettingsFileName = ".local.settings";
		const string GodotKey = "godot";
		const string DefaultGodotValue = "godot.exe";
	}
}
