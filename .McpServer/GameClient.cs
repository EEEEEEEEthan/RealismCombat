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
	static (string path, StreamWriter writer) CreateLogStream()
	{
		var logDir = Path.Combine(Program.projectRoot, ".logs");
		if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
		var logName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
		var logPath = Path.Combine(logDir, $"{logName}.log");
		Log.Print($"游戏日志路径: {logPath}");
		var writer = new StreamWriter(new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read), new UTF8Encoding(false))
		{
			AutoFlush = true,
		};
		return (logPath, writer);
	}
	static void BuildProject()
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
		var process = Process.Start(psi);
		if (process is null)
		{
			Log.PrintError("无法启动dotnet build");
			throw new InvalidOperationException("无法启动 dotnet build");
		}
		var stdout = process.StandardOutput;
		var stderr = process.StandardError;
		var output = Task.Run(() =>
		{
			while (true)
			{
				var line = stdout.ReadLine();
				if (line is null) break;
				Log.Print($"[构建输出] {line}");
			}
		});
		var error = Task.Run(() =>
		{
			while (true)
			{
				var line = stderr.ReadLine();
				if (line is null) break;
				Log.Print($"[构建错误] {line}");
			}
		});
		process.WaitForExit();
		Task.WaitAll(output, error);
		var exit = process.ExitCode;
		process.Dispose();
		if (exit != 0)
		{
			Log.PrintError($"编译失败，退出码: {exit}");
			throw new InvalidOperationException($"编译失败，退出码: {exit}");
		}
		Log.Print("项目构建完成");
	}
	static int AllocateFreePort()
	{
		var l = new TcpListener(IPAddress.Loopback, 0);
		l.Start();
		var p = ((IPEndPoint)l.LocalEndpoint).Port;
		l.Stop();
		return p;
	}
	public readonly int port;
	public readonly string logFilePath;
	readonly object sync = new();
	readonly SemaphoreSlim sendLock = new(1, 1);
	readonly Process process;
	readonly TcpClient client;
	readonly NetworkStream stream;
	readonly BinaryReader reader;
	readonly BinaryWriter writer;
	readonly StreamWriter logWriter;
	readonly CancellationTokenSource cancellationTokenSource = new();
	bool disposed;
	public int ProcessId { get; private set; }
	public event Action? OnDisconnected;
	/// <summary>
	///     构造时：确保.local.settings与godot配置，编译项目，启动Godot并以--port=xxx运行，随后连接Server。
	/// </summary>
	public GameClient(int? preferredPort = null)
	{
		Log.Print("GameClient构造函数开始");
		var godotPath = Program.settings.GetValueOrDefault(Program.SettingKeys.godotPath, "");
		Log.Print($"找到Godot路径: {godotPath}");
		if (string.IsNullOrWhiteSpace(godotPath))
			throw new InvalidOperationException(
				"""
				错误: 未配置Godot路径
				请在 .local.settings 文件中设置 godot 为 Godot 可执行文件的完整路径
				例如: godot = C:\Program Files\Godot\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64.exe
				""");
		if (!File.Exists(godotPath))
			throw new FileNotFoundException(
				$"""
				错误: Godot可执行文件不存在: {godotPath}
				请检查 .local.settings 文件中的 godot 路径配置是否正确
				当前配置的路径不存在，请确保路径正确
				""");
		BuildProject();
		port = preferredPort ?? AllocateFreePort();
		(logFilePath, logWriter) = CreateLogStream();
		Log.Print($"分配端口: {port}");
		process = StartGodotProcess(godotPath, Program.projectRoot, port);
		Log.Print("等待游戏启动并建立连接...");
		(client, stream, reader, writer) = WaitForGameAndConnect(port, TimeSpan.FromSeconds(15));
		Log.Print("GameClient构造完成，连接成功");
		StartConnectionMonitor();
	}
	/// <summary>
	///     发送字符串指令并等待服务器返回字符串，超时返回提示。
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
			await Task
				.Run(() =>
				{
					writer.Write(command);
					writer.Flush();
				})
				.ConfigureAwait(false);
			var read = Task.Run(() => reader.ReadString());
			var completed = await Task.WhenAny(read, Task.Delay(timeoutMs)).ConfigureAwait(false);
			if (completed == read)
			{
				var response = await read.ConfigureAwait(false);
				Log.Print($"命令响应: {response}");
				return response;
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
			if (disposed) return;
			disposed = true;
			cancellationTokenSource.Cancel();
			reader.TryDispose();
			writer.TryDispose();
			stream.TryDispose();
			client.TryDispose();
			try
			{
				if (!process.HasExited)
				{
					Log.Print($"正在终止Godot进程 (PID: {ProcessId})");
					process.Kill();
					process.WaitForExit(3000);
				}
			}
			catch (Exception ex)
			{
				Log.PrintError("终止进程时发生异常", ex);
			}
			process.TryDispose();
			logWriter.TryDispose();
		}
		cancellationTokenSource.TryDispose();
		sendLock.TryDispose();
		Log.Print("GameClient资源释放完成");
	}
	void StartConnectionMonitor() =>
		Task.Run(async () =>
			{
				try
				{
					while (!cancellationTokenSource.Token.IsCancellationRequested)
					{
						await Task.Delay(2000, cancellationTokenSource.Token).ConfigureAwait(false);
						var disconnected = false;
						lock (sync)
						{
							if (disposed) break;
							try
							{
								if (client.Client.Poll(0, SelectMode.SelectRead))
								{
									var buffer = new byte[1];
									if (client.Client.Receive(buffer, SocketFlags.Peek) == 0) disconnected = true;
								}
							}
							catch
							{
								disconnected = true;
							}
						}
						if (disconnected)
						{
							Log.Print("检测到游戏连接已断开");
							OnDisconnected?.Invoke();
							break;
						}
					}
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					Log.PrintError("连接监控任务异常", ex);
				}
			},
			cancellationTokenSource.Token);
	Process StartGodotProcess(string godotPath, string projectRoot, int port)
	{
		Log.Print($"准备启动Godot进程，端口: {port}");
		var processStartInfo = new ProcessStartInfo
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
			var process = Process.Start(processStartInfo);
			if (process is null) throw new InvalidOperationException("无法启动Godot进程");
			ProcessId = process.Id;
			Log.Print($"Godot进程已启动，进程ID: {ProcessId}");
			DataReceivedEventHandler onOutput = (_, @event) =>
			{
				if (@event.Data is null) return;
				lock (sync)
				{
					try
					{
						logWriter.WriteLine(@event.Data);
					}
					catch (Exception e)
					{
						Log.PrintException(e);
					}
				}
			};
			process.OutputDataReceived += onOutput;
			process.ErrorDataReceived += onOutput;
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			return process;
		}
	}
	(TcpClient, NetworkStream, BinaryReader, BinaryWriter) WaitForGameAndConnect(int port, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
			try
			{
				var client = new TcpClient();
				var task = client.ConnectAsync(IPAddress.Loopback, port);
				if (!task.Wait(TimeSpan.FromMilliseconds(300)) || !client.Connected)
				{
					client.Dispose();
					Thread.Sleep(100);
					continue;
				}
				lock (sync)
				{
					var stream = client.GetStream();
					var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
					var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
					return (client, stream, reader, writer);
				}
			}
			catch (Exception)
			{
				Thread.Sleep(200);
			}
		throw new TimeoutException("连接游戏服务器超时");
	}
}
