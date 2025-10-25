using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RealismCombat.McpServer.Extensions;
namespace RealismCombat.McpServer;
/// <summary>
///     提供与游戏进程的生命周期和Socket通信的客户端封装（实例化）。
/// </summary>
public sealed class GameClient : IDisposable
{
	readonly object sync = new();
	readonly SemaphoreSlim sendLock = new(1, 1);
	TcpClient client;
	NetworkStream stream;
	StreamReader reader;
	StreamWriter writer;
	Process process;
	StreamWriter logWriter;
	public int Port { get; }
	public int ProcessId { get; private set; }
	public string LogFilePath { get; private set; } = string.Empty;
	/// <summary>
	///     构造时：确保.local.settings与godot配置，编译项目，启动Godot并以--port=xxx运行，随后连接Server。
	/// </summary>
	public GameClient(int? preferredPort = null)
	{
		Log.Print("GameClient构造函数开始");
		var godotPath = Program.settings.GetValueOrDefault(Program.SettingKeys.godotPath, "");
		Log.Print($"找到Godot路径: {godotPath}");
		BuildProject();
		Port = preferredPort ?? AllocateFreePort();
		Log.Print($"分配端口: {Port}");
		StartGodotProcess(godotPath, Program.projectRoot, Port);
		Log.Print("等待游戏启动并建立连接...");
		var connected = WaitForGameAndConnect(Port, TimeSpan.FromSeconds(15));
		if (!connected)
		{
			Log.PrintError("连接游戏超时");
			Dispose();
			throw new InvalidOperationException("启动游戏失败或连接超时");
		}
		Log.Print("GameClient构造完成，连接成功");
	}
	/// <summary>
	///     发送字符串指令并等待服务器返回一行文本，超时返回提示。
	/// </summary>
	public async Task<string> SendCommand(string command, int timeoutMs)
	{
		Log.Print($"发送命令: {command}");
		await sendLock.WaitAsync().ConfigureAwait(false);
		try
		{
			lock (sync)
			{
				if (!client.Connected)
				{
					Log.PrintError("未连接到游戏");
					return "未连接到游戏";
				}
			}
			await writer.WriteLineAsync(command).ConfigureAwait(false);
			await writer.FlushAsync().ConfigureAwait(false);
			var read = reader.ReadLineAsync();
			var completed = await Task.WhenAny(read, Task.Delay(timeoutMs)).ConfigureAwait(false);
			if (completed == read)
			{
				var line = await read.ConfigureAwait(false);
				Log.Print($"命令响应: {line ?? "(空)"}");
				return line ?? "";
			}
			Log.PrintError($"命令超时: {command}");
			return "请求超时";
		}
		catch (Exception ex)
		{
			Log.PrintError($"发送命令异常: {command}", ex);
			return $"错误: {ex.Message}";
		}
		finally
		{
			sendLock.Release();
		}
	}
	/// <summary>
	///     释放连接。按要求：客户端断开后游戏应自行退出。
	/// </summary>
	public void Dispose()
	{
		Log.Print("开始释放GameClient资源");
		lock (sync)
		{
			reader.TryDispose();
			writer.TryDispose();
			stream.TryDispose();
			client.TryDispose();
			process.TryDispose();
			logWriter.TryDispose();
		}
		sendLock.Dispose();
		Log.Print("GameClient资源释放完成");
	}
	string? ReadSetting(string settingsPath, string key)
	{
		foreach (var raw in File.ReadAllLines(settingsPath, Encoding.UTF8))
		{
			var line = raw.Trim();
			if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
			var idx = line.IndexOf('=');
			if (idx <= 0) continue;
			var k = line[..idx].Trim();
			if (!k.Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
			return line[(idx + 1)..].Trim();
		}
		return null;
	}
	int AllocateFreePort()
	{
		var l = new TcpListener(IPAddress.Loopback, 0);
		l.Start();
		var p = ((IPEndPoint)l.LocalEndpoint).Port;
		l.Stop();
		return p;
	}
	void StartGodotProcess(string godotPath, string projectRoot, int port)
	{
		Log.Print($"准备启动Godot进程，端口: {port}");
		var logDir = Path.Combine(projectRoot, ".logs");
		try
		{
			Directory.CreateDirectory(logDir);
		}
		catch { }
		var logName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		var logPath = Path.Combine(logDir, $"{logName}.log");
		LogFilePath = logPath;
		Log.Print($"游戏日志路径: {logPath}");
		var psi = new ProcessStartInfo
		{
			FileName = godotPath,
			Arguments = $"--path \"{projectRoot}\" -- --port={port}",
			WorkingDirectory = projectRoot,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		lock (sync)
		{
			process.TryDispose();
			logWriter.TryDispose();
			try
			{
				logWriter = new(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read), new UTF8Encoding(false)) { AutoFlush = true, };
			}
			catch (Exception e)
			{
				Log.PrintException(e);
			}
			process = Process.Start(psi);
			if (process != null)
			{
				ProcessId = process.Id;
				Log.Print($"Godot进程已启动，进程ID: {ProcessId}");
				process.OutputDataReceived += (_, e) =>
				{
					if (e.Data is null) return;
					lock (sync)
					{
						try
						{
							logWriter?.WriteLine(e.Data);
						}
						catch { }
					}
				};
				process.ErrorDataReceived += (_, e) =>
				{
					if (e.Data is null) return;
					lock (sync)
					{
						try
						{
							logWriter?.WriteLine(e.Data);
						}
						catch { }
					}
				};
				try
				{
					process.BeginOutputReadLine();
				}
				catch { }
				try
				{
					process.BeginErrorReadLine();
				}
				catch { }
			}
		}
	}
	void BuildProject()
	{
		var projectRoot = Program.projectRoot;
		var slnPath = $"{projectRoot}/RealismCombat.sln";
		Log.Print($"开始构建项目: {slnPath}");
		var psi = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"build \"{slnPath}\" --nologo --verbosity quiet",
			WorkingDirectory = projectRoot,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using var process = Process.Start(psi);
		if (process is null)
		{
			Log.PrintError("无法启动dotnet build");
			throw new InvalidOperationException("无法启动 dotnet build");
		}
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			var error = process.StandardError.ReadToEnd();
			Log.PrintError($"编译失败: {error}");
			throw new InvalidOperationException($"编译失败: {error}");
		}
		Log.Print("项目构建完成");
	}
	bool WaitForGameAndConnect(int port, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
			try
			{
				var c = new TcpClient();
				var task = c.ConnectAsync(IPAddress.Loopback, port);
				if (!task.Wait(TimeSpan.FromMilliseconds(300)) || !c.Connected)
				{
					c.Dispose();
					Thread.Sleep(100);
					continue;
				}
				lock (sync)
				{
					client = c;
					stream = c.GetStream();
					reader = new(stream, Encoding.UTF8, false, 1024, leaveOpen: true);
					writer = new(stream, new UTF8Encoding(false)) { AutoFlush = true, };
				}
				Log.Print($"成功连接到游戏服务器，端口: {port}");
				return true;
			}
			catch (Exception)
			{
				Thread.Sleep(200);
			}
		return false;
	}
}
