using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
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
		if (LaunchArgs.port.HasValue) AddChild(new CommandHandlerNode(this));
		StartGameLoop();
	}
	async void StartGameLoop()
	{
		var menu = DialogueManager.CreateMenuDialogue();
		menu.AddOption(new() { title = "开始游戏", description = "开始新的冒险", });
		menu.AddOption(new() { title = "退出游戏", description = "关闭游戏程序", });
		while (true)
		{
			var choice = await menu;
			if (choice == 0)
			{
				Log.Print("[ProgramRoot] 玩家选择开始游戏");
			}
			else
			{
				Log.Print("[ProgramRoot] 玩家选择退出游戏");
				GetTree().Quit();
			}
			await Task.Delay(100);
		}
	}
}
