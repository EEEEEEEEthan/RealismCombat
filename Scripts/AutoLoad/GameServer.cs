using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.AutoLoad;
/// <summary>
///     游戏端TCP服务器，用于接收MCP客户端的连接和命令
/// </summary>
public sealed partial class GameServer : Node
{
	readonly object sync = new();
	int port;
	TcpListener? listener;
	CancellationTokenSource? cancellationTokenSource;
	LogListener? logListener;
	TcpClient? client;
	NetworkStream? stream;
	BinaryReader? reader;
	BinaryWriter? writer;
	bool disposed;
	TaskCompletionSource<bool>? responseTask;
	bool ClientIsConnected => client?.Connected ?? false;
	public event Action? OnConnected;
	public event Action? OnDisconnected;
	public event Action<string>? OnCommandReceived;
	public override void _Ready()
	{
		if (!LaunchArgs.port.HasValue)
		{
			Log.Print("[GameServer] 未指定端口，服务器不启动");
			return;
		}
		port = LaunchArgs.port.Value;
		listener = new(IPAddress.Loopback, port);
		cancellationTokenSource = new();
		logListener = new();
		Log.Print($"[GameServer] 初始化服务器，端口: {port}");
		try
		{
			listener.Start();
			Log.Print($"[GameServer] 服务器启动成功，监听端口: {port}");
			Task.Run(AcceptLoop, cancellationTokenSource.Token);
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
			throw;
		}
	}
	public override void _ExitTree()
	{
		if (disposed) return;
		disposed = true;
		Log.Print("[GameServer] 开始快速释放服务器资源");
		try
		{
			cancellationTokenSource?.Cancel();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		CloseClient();
		try
		{
			listener?.Stop();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		logListener?.TryDispose();
		Log.Print("[GameServer] 服务器资源释放完成");
	}
	public bool SendResponse()
	{
		var result = SendResponseInternal();
		responseTask?.TrySetResult(true);
		return result;
	}
	bool SendResponseInternal()
	{
		lock (sync)
		{
			if (writer == null || !ClientIsConnected) return false;
			var logs = logListener?.StopCollecting() ?? "";
			try
			{
				writer.Write(logs);
				writer.Flush();
				Log.Print($"[GameServer] 发送响应: {logs}");
				return true;
			}
			catch (Exception e)
			{
				Log.PrintException(e);
				return false;
			}
		}
	}
	async Task AcceptLoop()
	{
		if (cancellationTokenSource == null || listener == null) return;
		try
		{
			while (!cancellationTokenSource.Token.IsCancellationRequested)
			{
				Log.Print("[GameServer] 等待客户端连接...");
				var acceptedClient = await listener.AcceptTcpClientAsync(cancellationTokenSource.Token);
				lock (sync)
				{
					if (client != null)
					{
						Log.Print("[GameServer] 已有客户端连接，拒绝新连接");
						acceptedClient.Close();
						continue;
					}
					client = acceptedClient;
					stream = client.GetStream();
					reader = new(stream, Encoding.UTF8, leaveOpen: true);
					writer = new(stream, Encoding.UTF8, leaveOpen: true);
					Log.Print("[GameServer] 客户端连接成功");
				}
				OnConnected?.Invoke();
				_ = Task.Run(HandleClient, cancellationTokenSource.Token);
			}
		}
		catch (OperationCanceledException)
		{
			Log.Print("[GameServer] 接受循环已取消");
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
	}
	async Task HandleClient()
	{
		try
		{
			var cts = cancellationTokenSource;
			if (cts == null) return;
			try
			{
				while (!cts.Token.IsCancellationRequested)
				{
					string command;
					lock (sync)
					{
						if (reader == null || !ClientIsConnected)
						{
							Log.Print("[GameServer] 客户端已断开");
							break;
						}
						try
						{
							command = reader.ReadString();
						}
						catch (Exception ex)
						{
							Log.PrintException(ex);
							break;
						}
					}
					Log.Print($"[GameServer] 收到命令: {command}");
					responseTask = new();
					logListener?.StartCollecting();
					OnCommandReceived?.Invoke(command);
					var timeoutTask = Task.Delay(5000, cts.Token);
					var completedTask = await Task.WhenAny(responseTask.Task, timeoutTask);
					if (completedTask == timeoutTask)
					{
						Log.PrintErr("[GameServer] 等待响应超时");
						SendResponseInternal();
					}
				}
			}
			catch (Exception e)
			{
				Log.PrintException(e);
			}
			finally
			{
				CloseClient();
			}
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
	}
	void CloseClient()
	{
		reader?.TryDispose();
		writer?.TryDispose();
		stream?.TryDispose();
		client?.TryDispose();
		reader = null;
		writer = null;
		stream = null;
		client = null;
		Log.Print("[GameServer] 客户端连接已关闭");
		OnDisconnected?.Invoke();
	}
}
