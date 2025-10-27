using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Godot;
using RealismCombat.StateMachine;
using RealismCombat.StateMachine.ProgramStates;
namespace RealismCombat.Nodes;
partial class ProgramRootNode : Node, IStateOwner
{
	static readonly IReadOnlyDictionary<string, string> arguments;
	static ProgramRootNode()
	{
		var dict = new Dictionary<string, string>();
		var regex = new Regex(@"--(\S+)=(\S+)");
		foreach (var arg in OS.GetCmdlineUserArgs())
		{
			var match = regex.Match(arg);
			if (match.Success) dict[match.Groups[1].ToString()] = match.Groups[2].ToString();
		}
		arguments = dict;
	}
	readonly McpHandler? mcpHandler;
	[Export] Container dialogues = null!;
	public bool HadClientConnected { get; private set; }
	public double TotalTime { get; private set; }
	public int FrameCount { get; private set; }
	public State State { get; private set; }
	State IStateOwner.State
	{
		get => State;
		set => State = value;
	}
	ProgramRootNode()
	{
		State = new MenuState(this);
		if (arguments.TryGetValue(key: "port", value: out var portText))
			if (int.TryParse(s: portText, result: out var port))
			{
				Log.Print($"启动服务器，端口: {port}");
				mcpHandler = new(programRootNode: this, port: port);
				mcpHandler.OnClientConnected += OnClientConnected;
				mcpHandler.OnClientDisconnected += OnClientDisconnected;
				Log.Print($"服务器已启动，监听端口 {port}");
			}
			else
			{
				Log.PrintError($"端口号非法: {port}");
			}
		else
			GD.PrintErr("[GameRoot] 未提供 --port 参数，服务器未启动");
	}
	public void McpCheckPoint()
	{
		Log.Print(State.Status);
		mcpHandler?.McpCheckPoint();
	}
	public override void _Process(double delta)
	{
		TotalTime += delta;
		FrameCount++;
		mcpHandler?.Update();
		State.Update(delta);
		MoveChild(childNode: dialogues, toIndex: GetChildCount() - 1);
	}
	public DialogueNode ShowDialogue(string text, params (string, Action)[] options)
	{
		var dialogue = DialogueNode.Show(label: text, options: options);
		dialogues.AddChild(dialogue);
		return dialogue;
	}
	/// <summary>
	///     显示对话并异步返回所选项的索引。
	/// </summary>
	/// <param name="text">对话文本</param>
	/// <param name="options">选项文本</param>
	/// <returns>所选项索引的任务</returns>
	public Task<int> ShowDialogue(string text, params string[] options) => ShowDialogue(text: text, timeout: null, options: options);
	/// <summary>
	///     显示对话并异步返回所选项的索引，支持可选超时。
	/// </summary>
	/// <param name="text">对话文本</param>
	/// <param name="timeout">超时时间（秒）。为 null 表示不使用超时</param>
	/// <param name="options">选项文本</param>
	/// <returns>所选项索引的任务</returns>
	public Task<int> ShowDialogue(string text, float? timeout, params string[] options)
	{
		var tcs = new TaskCompletionSource<int>();
		var noOptionsProvided = options == null || options.Length == 0;
		var effectiveOptions = noOptionsProvided ? new[] { "继续", } : options!;
		var effectiveTimeout = timeout;
		if (noOptionsProvided && mcpHandler != null) effectiveTimeout = 3f;
		Timer? timeoutTimer = null;
		var optionTuples = new (string, Action)[effectiveOptions.Length];
		DialogueNode dialogue = null;
		for (var i = 0; i < effectiveOptions.Length; i++)
		{
			var index = i;
			optionTuples[i] = (effectiveOptions[i], () =>
			{
				dialogue.QueueFree();
				if (tcs.TrySetResult(index))
					if (timeoutTimer != null && IsInstanceValid(timeoutTimer))
						timeoutTimer.QueueFree();
			});
		}
		dialogue = ShowDialogue(text: text, options: optionTuples);
		if (effectiveTimeout != null)
		{
			var seconds = (double)effectiveTimeout.Value;
			if (seconds <= 0)
			{
				// 立即选择第一个
				if (tcs.TrySetResult(0))
					if (IsInstanceValid(dialogue))
						dialogue.QueueFree();
			}
			else
			{
				timeoutTimer = new() { OneShot = true, Autostart = true, WaitTime = seconds, };
				AddChild(timeoutTimer);
				timeoutTimer.Timeout += () =>
				{
					if (tcs.TrySetResult(0))
						if (IsInstanceValid(dialogue))
							dialogue.QueueFree();
					timeoutTimer.QueueFree();
				};
			}
		}
		return tcs.Task;
	}
	void _QuitGame() => GetTree().Quit();
	void OnClientConnected() => HadClientConnected = true;
	void OnClientDisconnected()
	{
		if (HadClientConnected) GetTree().Quit();
	}
}
