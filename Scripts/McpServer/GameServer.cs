using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace RealismCombat.McpServer;
/// <summary>
///     游戏端TCP服务器，用于接收MCP客户端的连接和命令
/// </summary>
public sealed class GameServer : IDisposable
{
	readonly int port;
	readonly TcpListener listener;
	readonly CancellationTokenSource cancellationTokenSource = new();
	readonly object sync = new();
	TcpClient? client;
	NetworkStream? stream;
	BinaryReader? reader;
	BinaryWriter? writer;
	bool disposed;
	public bool IsConnected
	{
		get
		{
			lock (sync)
			{
				return client?.Connected ?? false;
			}
		}
	}
	public event Action? OnConnected;
	public event Action? OnDisconnected;
	public event Action<string, Action<string>>? OnCommandReceived;
	public GameServer(int port)
	{
		this.port = port;
		listener = new(IPAddress.Loopback, port);
		Log.Print($"[GameServer] 创建服务器，端口: {port}");
	}
	public void Start()
	{
		try
		{
			listener.Start();
			Log.Print($"[GameServer] 服务器启动成功，监听端口: {port}");
			_ = Task.Run(AcceptLoop, cancellationTokenSource.Token);
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
			throw;
		}
	}
	public void Dispose()
	{
		lock (sync)
		{
			if (disposed) return;
			disposed = true;
		}
		Log.Print("[GameServer] 开始快速释放服务器资源");
		try
		{
			cancellationTokenSource.Cancel();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		try
		{
			CloseClient();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		try
		{
			listener.Stop();
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		Log.Print("[GameServer] 服务器资源释放完成");
	}
	async Task AcceptLoop()
	{
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
			while (!cancellationTokenSource.Token.IsCancellationRequested)
			{
				string command;
				lock (sync)
				{
					if (reader == null || !IsConnected)
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
				var responseReceived = false;
				var response = "未处理的命令";
				if (OnCommandReceived != null)
				{
					OnCommandReceived.Invoke(command,
						resp =>
						{
							response = resp;
							responseReceived = true;
						});
					var timeout = DateTime.UtcNow.AddSeconds(5);
					while (!responseReceived && DateTime.UtcNow < timeout) await Task.Delay(50, cancellationTokenSource.Token);
					if (!responseReceived)
					{
						response = "命令处理超时";
						Log.PrintErr($"[GameServer] 命令处理超时: {command}");
					}
				}
				lock (sync)
				{
					if (writer != null && IsConnected)
						try
						{
							writer.Write(response);
							writer.Flush();
							Log.Print($"[GameServer] 发送响应: {response}");
						}
						catch (Exception ex)
						{
							Log.PrintException(ex);
							break;
						}
				}
			}
		}
		catch (OperationCanceledException)
		{
			Log.Print("[GameServer] 客户端处理已取消");
		}
		catch (Exception ex)
		{
			Log.PrintException(ex);
		}
		finally
		{
			CloseClient();
		}
	}
	void CloseClient()
	{
		lock (sync)
		{
			reader?.Dispose();
			writer?.Dispose();
			stream?.Dispose();
			client?.Close();
			reader = null;
			writer = null;
			stream = null;
			client = null;
			Log.Print("[GameServer] 客户端连接已关闭");
		}
		OnDisconnected?.Invoke();
	}
}
