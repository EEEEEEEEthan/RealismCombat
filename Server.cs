using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
	readonly CancellationTokenSource cts = new();
	readonly object sync = new();
	readonly object writeSync = new();
	readonly Task acceptLoop;
	TcpClient client;
	NetworkStream stream;
	StreamReader reader;
	StreamWriter writer;
	public string? PendingRequest { get; private set; }
	public event Action? OnClientConnected;
	public event Action? OnClientDisconnected;
	public Server(int port)
	{
		listener = new(localaddr: IPAddress.Loopback, port: port);
		listener.Start();
		acceptLoop = Task.Run(AcceptLoopAsync);
	}
	public void Respond(string response)
	{
		lock (sync)
		{
			if (client is null || stream is null || writer is null) return;
			if (PendingRequest is null) return;
			lock (writeSync)
			{
				writer.WriteLine(response);
				writer.Flush();
			}
			PendingRequest = null;
		}
	}
	public void Stop()
	{
		cts.Cancel();
		try
		{
			listener.Stop();
		}
		catch { }
		try
		{
			acceptLoop?.Wait(500);
		}
		catch { }
	}
	async Task AcceptLoopAsync()
	{
		var token = cts.Token;
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
						reader = new(stream: stream, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
						writer = new(stream: stream, encoding: new UTF8Encoding(false)) { AutoFlush = true, };
						accepted = true;
					}
				}
				if (!accepted)
				{
					try
					{
						client.Close();
					}
					catch { }
					continue;
				}
				OnClientConnected?.Invoke();
				Task.Run(ReadLoopAsync);
			}
		}
		catch (OperationCanceledException) { }
	}
	async Task ReadLoopAsync()
	{
		var token = cts.Token;
		try
		{
			while (!token.IsCancellationRequested)
			{
				var line = await reader!.ReadLineAsync().ConfigureAwait(false);
				if (line is null) break;
				var trimmed = line.Trim();
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
						writer!.WriteLine("正忙");
						writer!.Flush();
					}
			}
		}
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
			try
			{
				reader?.Dispose();
			}
			catch { }
			try
			{
				writer?.Dispose();
			}
			catch { }
			try
			{
				stream?.Dispose();
			}
			catch { }
			try
			{
				client?.Close();
			}
			catch { }
			client = null;
			stream = null;
			reader = null;
			writer = null;
			PendingRequest = null;
		}
		OnClientDisconnected?.Invoke();
	}
}
