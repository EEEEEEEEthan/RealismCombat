using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace RealismCombat.McpServer;
/// <summary>
///     提供与游戏进程的生命周期和Socket通信的客户端封装（实例化）。
/// </summary>
public sealed class GameClient : IDisposable
{
	readonly object _sync = new();
	readonly SemaphoreSlim _sendLock = new(1, 1);
	readonly string _projectRoot;
	TcpClient _client;
	NetworkStream _stream;
	StreamReader _reader;
	StreamWriter _writer;
	Process _process;
	StreamWriter _logWriter;
	public int Port { get; }
	public int ProcessId { get; private set; }
	public string LogFilePath { get; private set; } = string.Empty;
	/// <summary>
	///     构造时：确保.local.settings与godot配置，编译项目，启动Godot并以--port=xxx运行，随后连接Server。
	/// </summary>
	public GameClient(int? preferredPort = null)
	{
		_projectRoot = EnsureProjectRoot();
		var settingsPath = Path.Combine(_projectRoot, ".local.settings");
		EnsureLocalSettings(settingsPath);
		var godotPath = ReadSetting(settingsPath, "godot");
		if (string.IsNullOrWhiteSpace(godotPath) || !File.Exists(godotPath))
			throw new InvalidOperationException("未配置或找不到godot路径，请在 .local.settings 中设置 godot = <Godot可执行路径>");
		BuildProject(_projectRoot);
		Port = preferredPort ?? AllocateFreePort();
		StartGodotProcess(godotPath, _projectRoot, Port);
		var connected = WaitForGameAndConnect(Port, TimeSpan.FromSeconds(15));
		if (!connected)
		{
			Dispose();
			throw new InvalidOperationException("启动游戏失败或连接超时");
		}
	}
	/// <summary>
	///     发送字符串指令并等待服务器返回一行文本，超时返回提示。
	/// </summary>
	public async Task<string> SendCommand(string command, int timeoutMs)
	{
		await _sendLock.WaitAsync().ConfigureAwait(false);
		try
		{
			lock (_sync)
			{
				if (_client is null || _stream is null || _reader is null || _writer is null || !_client.Connected) return "未连接到游戏";
			}
			await _writer!.WriteLineAsync(command).ConfigureAwait(false);
			await _writer.FlushAsync().ConfigureAwait(false);
			var readTask = _reader!.ReadLineAsync();
			var completed = await Task.WhenAny(readTask, Task.Delay(timeoutMs)).ConfigureAwait(false);
			if (completed == readTask)
			{
				var line = await readTask.ConfigureAwait(false);
				return line ?? "";
			}
			return "请求超时";
		}
		catch (Exception ex)
		{
			return $"错误: {ex.Message}";
		}
		finally
		{
			_sendLock.Release();
		}
	}
	/// <summary>
	///     释放连接。按要求：客户端断开后游戏应自行退出。
	/// </summary>
	public void Dispose()
	{
		lock (_sync)
		{
			try
			{
				_reader.Dispose();
			}
			catch { }
			try
			{
				_writer.Dispose();
			}
			catch { }
			try
			{
				_stream.Dispose();
			}
			catch { }
			try
			{
				_client.Close();
			}
			catch { }
			try
			{
				_process.CancelOutputRead();
			}
			catch { }
			try
			{
				_process.CancelErrorRead();
			}
			catch { }
			try
			{
				_process.Dispose();
			}
			catch { }
			_process = null;
			try
			{
				_logWriter.Dispose();
			}
			catch
			{
				// ignored
			}
			_logWriter = null;
		}
		_sendLock.Dispose();
	}
	string EnsureProjectRoot()
	{
		var cwd = Directory.GetCurrentDirectory();
		if (File.Exists(Path.Combine(cwd, "project.godot"))) return cwd;
		var dir = new DirectoryInfo(cwd);
		while (dir is not null)
		{
			if (File.Exists(Path.Combine(dir.FullName, "project.godot"))) return dir.FullName;
			dir = dir.Parent;
		}
		return cwd;
	}
	void EnsureLocalSettings(string settingsPath)
	{
		if (!File.Exists(settingsPath))
		{
			File.WriteAllText(settingsPath, "godot = \r\n", new UTF8Encoding(false));
			return;
		}
		var content = File.ReadAllText(settingsPath, Encoding.UTF8);
		if (!content.Split('\n').Any(l => l.TrimStart().StartsWith("godot", StringComparison.OrdinalIgnoreCase)))
			File.AppendAllText(settingsPath, "godot = \r\n");
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
		var logDir = Path.Combine(projectRoot, ".logs");
		try
		{
			Directory.CreateDirectory(logDir);
		}
		catch { }
		var logName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		var logPath = Path.Combine(logDir, $"{logName}.log");
		LogFilePath = logPath;
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
		lock (_sync)
		{
			try
			{
				_process?.Dispose();
			}
			catch { }
			try
			{
				_logWriter?.Dispose();
			}
			catch { }
			try
			{
				_logWriter = new(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read), new UTF8Encoding(false)) { AutoFlush = true, };
			}
			catch { }
			_process = Process.Start(psi);
			if (_process != null)
			{
				ProcessId = _process.Id;
				_process.OutputDataReceived += (_, e) =>
				{
					if (e.Data is null) return;
					lock (_sync)
					{
						try
						{
							_logWriter?.WriteLine(e.Data);
						}
						catch { }
					}
				};
				_process.ErrorDataReceived += (_, e) =>
				{
					if (e.Data is null) return;
					lock (_sync)
					{
						try
						{
							_logWriter?.WriteLine(e.Data);
						}
						catch { }
					}
				};
				try
				{
					_process.BeginOutputReadLine();
				}
				catch { }
				try
				{
					_process.BeginErrorReadLine();
				}
				catch { }
			}
		}
	}
	void BuildProject(string projectRoot)
	{
		var csprojFiles = Directory.GetFiles(projectRoot, "*.csproj");
		if (csprojFiles.Length == 0) throw new InvalidOperationException("未找到 .csproj 文件");
		var csprojPath = csprojFiles[0];
		var binDebugPath = Path.Combine(projectRoot, ".godot", "mono", "temp", "bin", "Debug");
		if (Directory.Exists(binDebugPath))
		{
			var binFiles = Directory.GetFiles(binDebugPath, "*.dll");
			if (binFiles.Length > 0)
			{
				var lastBuildTime = binFiles.Max(f => File.GetLastWriteTimeUtc(f));
				var csprojModTime = File.GetLastWriteTimeUtc(csprojPath);
				var csFiles = Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories);
				var latestCsModTime = csFiles.Length > 0 ? csFiles.Max(f => File.GetLastWriteTimeUtc(f)) : DateTime.MinValue;
				if (lastBuildTime > csprojModTime && lastBuildTime > latestCsModTime) return;
			}
		}
		var psi = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"build \"{csprojPath}\" --nologo --verbosity quiet",
			WorkingDirectory = projectRoot,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using var process = Process.Start(psi);
		if (process is null) throw new InvalidOperationException("无法启动 dotnet build");
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			var error = process.StandardError.ReadToEnd();
			throw new InvalidOperationException($"编译失败: {error}");
		}
	}
	bool WaitForGameAndConnect(int port, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
			try
			{
				var c = new TcpClient();
				var task = c.ConnectAsync(IPAddress.Loopback, port);
				if (!task.Wait(TimeSpan.FromMilliseconds(300)))
				{
					c.Dispose();
					Thread.Sleep(100);
					continue;
				}
				if (!c.Connected)
				{
					c.Dispose();
					Thread.Sleep(100);
					continue;
				}
				lock (_sync)
				{
					_client = c;
					_stream = c.GetStream();
					_reader = new(_stream, Encoding.UTF8, false, 1024, leaveOpen: true);
					_writer = new(_stream, new UTF8Encoding(false)) { AutoFlush = true, };
				}
				return true;
			}
			catch (Exception)
			{
				Thread.Sleep(200);
			}
		return false;
	}
}
