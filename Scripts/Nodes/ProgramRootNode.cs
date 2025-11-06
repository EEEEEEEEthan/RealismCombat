using Godot;
namespace RealismCombat.Nodes;
/// <summary>
///     程序根节点，负责初始化游戏生命周期
/// </summary>
public partial class ProgramRootNode : Node
{
	public override void _Ready()
	{
		Log.Print("[ProgramRoot] 程序启动");
		var godotPath = Settings.Get("godot");
		if (godotPath != null) Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
		if (LaunchArgs.port.HasValue)
		{
			var commandHandler = new CommandHandler(this);
			AddChild(commandHandler);
			commandHandler.SetupServerCallbacks();
		}
	}
	public override void _ExitTree() => Log.Print("[ProgramRoot] 程序退出完成");
}
