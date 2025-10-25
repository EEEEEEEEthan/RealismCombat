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
	readonly int _port;
	readonly TcpListener _listener;
	readonly CancellationTokenSource _cts = new();
	readonly object _sync = new();
	readonly object _writeSync = new();
	TcpClient? _client;
	NetworkStream? _stream;
	StreamReader? _reader;
	StreamWriter? _writer;
	readonly Task? _acceptLoopTask;
	Task? _readLoopTask;
	public string? PendingRequest { get; private set; }
	public event Action? ClientConnected;
	public event Action? ClientDisconnected;
	public Server(int port)
	{
		_port = port;
		_listener = new(localaddr: IPAddress.Loopback, port: _port);
		_listener.Start();
		_acceptLoopTask = Task.Run(AcceptLoopAsync);
	}
	public void Respond(string response)
	{
		lock (_sync)
		{
			if (_client is null || _stream is null || _writer is null) return;
			if (PendingRequest is null) return;
			lock (_writeSync)
			{
				_writer.WriteLine(response);
				_writer.Flush();
			}
			PendingRequest = null;
		}
	}
	public void Stop()
	{
		_cts.Cancel();
		try
		{
			_listener.Stop();
		}
		catch { }
		try
		{
			_acceptLoopTask?.Wait(500);
		}
		catch { }
	}
	async Task AcceptLoopAsync()
	{
		var token = _cts.Token;
		try
		{
			while (!token.IsCancellationRequested)
			{
				var client = await _listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
				var accepted = false;
				lock (_sync)
				{
					if (_client is null)
					{
						_client = client;
						_stream = client.GetStream();
						_reader = new(stream: _stream, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
						_writer = new(stream: _stream, encoding: new UTF8Encoding(false)) { AutoFlush = true, };
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
				ClientConnected?.Invoke();
				_readLoopTask = Task.Run(ReadLoopAsync);
			}
		}
		catch (OperationCanceledException) { }
	}
	async Task ReadLoopAsync()
	{
		var token = _cts.Token;
		try
		{
			while (!token.IsCancellationRequested)
			{
				var line = await _reader!.ReadLineAsync().ConfigureAwait(false);
				if (line is null) break;
				var trimmed = line.Trim();
				if (trimmed.Length == 0) continue;
				bool isBusy;
				lock (_sync)
				{
					isBusy = PendingRequest is not null;
					if (!isBusy) PendingRequest = trimmed;
				}
				if (isBusy)
					lock (_writeSync)
					{
						_writer!.WriteLine("正忙");
						_writer!.Flush();
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
		lock (_sync)
		{
			try
			{
				_reader?.Dispose();
			}
			catch { }
			try
			{
				_writer?.Dispose();
			}
			catch { }
			try
			{
				_stream?.Dispose();
			}
			catch { }
			try
			{
				_client?.Close();
			}
			catch { }
			_client = null;
			_stream = null;
			_reader = null;
			_writer = null;
			PendingRequest = null;
		}
		ClientDisconnected?.Invoke();
	}
}
