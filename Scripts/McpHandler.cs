using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RealismCombat.Extensions;
using RealismCombat.Nodes;
namespace RealismCombat;
/// <summary>
///     简单的单客户端TCP服务器：
///     - 仅允许一个客户端连接
///     - 一次仅处理一条指令，其余直接回复"正忙"
///     - 客户端断开时触发事件
/// </summary>
class McpHandler
{
	class CommandLifeCycle : IDisposable
	{
		readonly List<string> messages = new();
		readonly Action<string> onLog;
		public string Message => string.Join(separator: "\n", values: messages);
		public CommandLifeCycle()
		{
			onLog = s => messages.Add(s);
			Log.OnLog += onLog;
			Log.OnError += onLog;
		}
		public void Dispose()
		{
			Log.OnLog -= onLog;
			Log.OnError -= onLog;
		}
	}
	readonly TcpListener listener;
	readonly CancellationTokenSource cancellationTokenSource = new();
	readonly object sync = new();
	readonly object writeSync = new();
	readonly ProgramRootNode programRootNode;
	TcpClient? client;
	NetworkStream? stream;
	BinaryReader? reader;
	BinaryWriter? writer;
	string? pendingCommand;
	CommandLifeCycle? commandLifeCycle;
	public event Action? OnClientConnected;
	public event Action? OnClientDisconnected;
	public McpHandler(ProgramRootNode programRootNode, int port)
	{
		this.programRootNode = programRootNode;
		listener = new(localaddr: IPAddress.Loopback, port: port);
		listener.Start();
		Task.Run(AcceptLoopAsync);
	}
	public void McpRespond()
	{
		if (commandLifeCycle != null)
		{
			var state = commandLifeCycle;
			commandLifeCycle = null;
			Log.Print(nameof(McpRespond));
			Respond(state.Message);
			state.Dispose();
		}
	}
	internal void Update()
	{
		if (pendingCommand != null)
			if (commandLifeCycle == null)
			{
				commandLifeCycle = new();
				try
				{
					Log.Print($"McpRequest {pendingCommand}");
					programRootNode.OnMcpRequest(pendingCommand);
				}
				catch (Exception e)
				{
					Log.PrintException(e);
					McpRespond();
				}
			}
	}
	void Respond(string response)
	{
		lock (sync)
		{
			if (client is null || stream is null || writer is null) return;
			if (pendingCommand is null) return;
			lock (writeSync)
			{
				writer.Write(response);
				writer.Flush();
			}
			pendingCommand = null;
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
						reader = new(input: stream, encoding: Encoding.UTF8, leaveOpen: true);
						writer = new(output: stream, encoding: Encoding.UTF8, leaveOpen: true);
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
				var command = await Task.Run(function: () => reader!.ReadString(), cancellationToken: token).ConfigureAwait(false);
				var trimmed = command.Trim();
				if (trimmed.Length == 0) continue;
				bool isBusy;
				lock (sync)
				{
					isBusy = pendingCommand is not null;
					if (!isBusy) pendingCommand = trimmed;
				}
				if (isBusy)
					lock (writeSync)
					{
						writer!.Write("正忙");
						writer!.Flush();
					}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
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
			pendingCommand = null;
		}
		OnClientDisconnected?.Invoke();
	}
}
