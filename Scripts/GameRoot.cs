using System;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Commands;
namespace RealismCombat;
public partial class GameRoot : Node
{
	readonly McpSocket? mcpSocket;
	public CommandHandler CommandHandler { get; }
	PrepareCanvas? PrepareCanvas { get; set; }
	GameRoot()
	{
		CommandHandler = new(this);
		try
		{
			mcpSocket = new(this);
		}
		catch (Exception e)
		{
			Log.PrintE(e);
		}
	}
	[Signal] public delegate void OnLogEventHandler(string message);
	public override void _Ready()
	{
		var scene = ResourceLoader.Load<PackedScene>(ResourceTable.prepareCanvas);
		PrepareCanvas = scene.Instantiate<PrepareCanvas>();
		AddChild(PrepareCanvas);
	}
	public override void _Process(double delta) => mcpSocket?.Update(delta);
	public async Task<string> ExecCommand(string command) => await CommandHandler.Handle(command);
	/// <summary>
	///     结束当前挂起的日志收集请求，并回复收集到的日志。
	/// </summary>
	public void MarkCheckPoint() => mcpSocket?.MarkCheckPoint();
	protected override void Dispose(bool disposing) => mcpSocket?.Dispose();
}
