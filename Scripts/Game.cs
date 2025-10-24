using System;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Game;
public partial class GameRoot : Node
{
	readonly McpSocket? mcpSocket;
	public Commands.CommandHandler CommandHandler { get; private set; }
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
	protected override void Dispose(bool disposing) => mcpSocket?.Dispose();
}
