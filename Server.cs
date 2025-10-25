using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RealismCombat.Extensions;
namespace RealismCombat;
/// <summary>
///     简单的单客户端TCP服务器：
///     - 仅允许一个客户端连接
///     - 一次仅处理一条指令，其余直接回复"正忙"
///     - 客户端断开时触发事件
/// </summary>
public class Server
{
	readonly TcpListener listener;
	readonly CancellationTokenSource cancellationTokenSource = new();
	readonly object sync = new();
	readonly object writeSync = new();
	TcpClient? client;
	NetworkStream? stream;
	BinaryReader? reader;
	BinaryWriter? writer;
	public string? PendingRequest { get; private set; }
	public event Action? OnClientConnected;
	public event Action? OnClientDisconnected;
	public Server(int port)
	{
		listener = new(localaddr: IPAddress.Loopback, port: port);
		listener.Start();
		Task.Run(AcceptLoopAsync);
	}
	public void Respond(string response)
	{
		lock (sync)
		{
			if (client is null || stream is null || writer is null) return;
			if (PendingRequest is null) return;
			lock (writeSync)
			{
				writer.Write(response);
				writer.Flush();
			}
			PendingRequest = null;
		}
	}
	async Task AcceptLoopAsync()
	{
		var token = cancellationTokenSource.Token;
		try
		{
			while (!token.IsCancellationRequested)
			{
				var client = await listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
				var accepted = false;
				lock (sync)
				{
					if (this.client is null)
					{
						this.client = client;
						stream = client.GetStream();
						reader = new(stream, Encoding.UTF8, leaveOpen: true);
						writer = new(stream, Encoding.UTF8, leaveOpen: true);
						accepted = true;
					}
				}
				if (!accepted)
				{
					try
					{
						client.Close();
					}
					catch (Exception e)
					{
						Log.PrintException(e);
					}
					continue;
				}
				OnClientConnected?.Invoke();
				_ = Task.Run(function: ReadLoopAsync, cancellationToken: token);
			}
		}
		catch (OperationCanceledException) { }
	}
	async Task ReadLoopAsync()
	{
		var token = cancellationTokenSource.Token;
		try
		{
			while (!token.IsCancellationRequested)
			{
				var command = await Task.Run(() => reader!.ReadString(), token).ConfigureAwait(false);
				var trimmed = command.Trim();
				if (trimmed.Length == 0) continue;
				bool isBusy;
				lock (sync)
				{
					isBusy = PendingRequest is not null;
					if (!isBusy) PendingRequest = trimmed;
				}
				if (isBusy)
					lock (writeSync)
					{
						writer!.Write("正忙");
						writer!.Flush();
					}
			}
		}
		catch (EndOfStreamException) { }
		catch (IOException) { }
		catch (ObjectDisposedException) { }
		finally
		{
			HandleClientDisconnected();
		}
	}
	void HandleClientDisconnected()
	{
		lock (sync)
		{
			reader?.TryDispose();
			writer?.TryDispose();
			stream?.TryDispose();
			client?.TryDispose();
			reader = null;
			writer = null;
			client = null;
			stream = null;
			PendingRequest = null;
		}
		OnClientDisconnected?.Invoke();
	}
}
