using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat;
class McpSocket : IDisposable
{
	readonly struct RequestItem(string message, TaskCompletionSource<string> taskCompletionSource)
	{
		public readonly string message = message;
		public readonly TaskCompletionSource<string> taskCompletionSource = taskCompletionSource;
	}
	readonly ConcurrentQueue<RequestItem> requestQueue = new();
	readonly GameRoot gameRoot;
	readonly TcpListener tcpListener;
	readonly CancellationTokenSource cancellationTokenSource;
	readonly object checkpointSync = new();
	readonly List<string> checkpointBuffer = [];
	TaskCompletionSource<string>? checkpointPendingTcs;
	int port = 9999;
	bool disposed;
	internal McpSocket(GameRoot gameRoot)
	{
		this.gameRoot = gameRoot;
		ParsePortFromArgs();
		cancellationTokenSource = new();
		tcpListener = new(localaddr: IPAddress.Any, port: port);
		tcpListener.Start();
		Log.Print("SocketServer: 成功启动，监听端口 ", port);
		_ = Task.Run(() => AcceptLoopAsync(cancellationTokenSource.Token));
	}
	internal void Update(double delta)
	{
		while (requestQueue.TryDequeue(out var item)) ProcessOneOnMainThread(item);
	}
	internal void Dispose()
	{
		if (disposed) return;
		disposed = true;
		try
		{
			cancellationTokenSource.Cancel();
			tcpListener.Stop();
		}
		catch (Exception e)
		{
			Log.Print(e);
		}
		TaskCompletionSource<string>? tcsToComplete = null;
		var resultToSend = string.Empty;
		lock (checkpointSync)
		{
			if (checkpointPendingTcs != null)
			{
				Log.OnLog -= OnLogCaptured;
				Log.OnError -= OnErrorCaptured;
				resultToSend = string.Join(separator: "\n", values: checkpointBuffer);
				checkpointBuffer.Clear();
				tcsToComplete = checkpointPendingTcs;
				checkpointPendingTcs = null;
			}
		}
		tcsToComplete?.TrySetResult(resultToSend);
		Log.PrintE($"{nameof(McpSocket)}已关闭");
	}
	/// <summary>
	///     结束当前挂起的日志收集请求，将收集到的所有日志行作为响应返回，并清理订阅与缓存。
	/// </summary>
	internal void MarkCheckPoint()
	{
		TaskCompletionSource<string>? tcsToComplete;
		string result;
		lock (checkpointSync)
		{
			if (checkpointPendingTcs is null) return;
			Log.OnLog -= OnLogCaptured;
			Log.OnError -= OnErrorCaptured;
			result = string.Join(separator: "\n", values: checkpointBuffer);
			checkpointBuffer.Clear();
			tcsToComplete = checkpointPendingTcs;
			checkpointPendingTcs = null;
		}
		tcsToComplete!.TrySetResult(result);
	}
	void IDisposable.Dispose() => Dispose();
	void OnLogCaptured(string text)
	{
		lock (checkpointSync)
		{
			if (checkpointPendingTcs != null) checkpointBuffer.Add(text);
		}
	}
	void OnErrorCaptured(string text)
	{
		lock (checkpointSync)
		{
			if (checkpointPendingTcs != null) checkpointBuffer.Add(text);
		}
	}
	void ParsePortFromArgs()
	{
		var args = OS.GetCmdlineArgs();
		foreach (var arg in args)
			if (arg.StartsWith("--port="))
			{
				var portStr = arg[7..];
				if (int.TryParse(s: portStr, result: out var p))
				{
					port = p;
					Log.Print("SocketServer: 从命令行参数解析端口号: ", port);
					return;
				}
			}
		Log.Print("SocketServer: 使用默认端口: ", port);
	}
	async Task AcceptLoopAsync(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				TcpClient client;
				try
				{
					client = await tcpListener.AcceptTcpClientAsync(token);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch
				{
					continue;
				}
				_ = HandleClientAsync(client: client, token: token);
			}
		}
		catch (Exception e)
		{
			Log.PrintE(e);
		}
	}
	async Task HandleClientAsync(TcpClient client, CancellationToken token)
	{
		using (client)
		{
			client.NoDelay = true;
			var stream = client.GetStream();
			var buffer = new byte[4096];
			var sb = new StringBuilder();
			var watch = Stopwatch.StartNew();
			const int timeoutMs = 5000;
			while (watch.ElapsedMilliseconds < timeoutMs && !token.IsCancellationRequested)
			{
				if (!client.Connected) break;
				if (stream.DataAvailable)
				{
					int read;
					try
					{
						read = await stream.ReadAsync(buffer: buffer.AsMemory(start: 0, length: buffer.Length), cancellationToken: token);
					}
					catch (OperationCanceledException)
					{
						break;
					}
					if (read <= 0) break;
					sb.Append(Encoding.UTF8.GetString(bytes: buffer, index: 0, count: read));
					if (sb.ToString().IndexOf('\n') >= 0) break;
				}
				else
				{
					try
					{
						await Task.Delay(millisecondsDelay: 10, cancellationToken: token);
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}
			}
			var message = sb.ToString().Trim();
			if (string.IsNullOrEmpty(message)) return;
			var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			requestQueue.Enqueue(new(message: message, taskCompletionSource: tcs));
			string response;
			try
			{
				response = await tcs.Task;
			}
			catch (Exception ex)
			{
				response = "error: " + ex.Message;
			}
			var bytes = Encoding.UTF8.GetBytes(response);
			try
			{
				await stream.WriteAsync(buffer: bytes.AsMemory(start: 0, length: bytes.Length), cancellationToken: token);
			}
			catch (Exception e)
			{
				Log.PrintE(e);
			}
		}
	}
	void ProcessOneOnMainThread(RequestItem item)
	{
		try
		{
			var started = false;
			lock (checkpointSync)
			{
				if (checkpointPendingTcs == null)
				{
					checkpointPendingTcs = item.taskCompletionSource;
					checkpointBuffer.Clear();
					Log.OnLog += OnLogCaptured;
					Log.OnError += OnErrorCaptured;
					started = true;
				}
			}
			if (!started)
			{
				item.taskCompletionSource.TrySetResult("正忙");
				return;
			}
			if (item.message == "system.shutdown")
			{
				Log.Print("游戏即将关闭");
				MarkCheckPoint();
				gameRoot.GetTree().CallDeferred("quit");
				return;
			}
			gameRoot.ExecCommand(item.message);
		}
		catch (Exception ex)
		{
			item.taskCompletionSource.TrySetResult("error: " + ex.Message);
		}
	}
	~McpSocket() => Dispose();
}
