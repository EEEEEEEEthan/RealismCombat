using System;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
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
			AddChild(new CommandHandlerNode(this));
		else
			StartGameLoop();
	}
	public async void StartGameLoop()
	{
		try
		{
			while (this.Valid())
			{
				var menu = DialogueManager.CreateMenuDialogue();
				menu.AddOption(new() { title = "开始游戏", description = "开始新的冒险", });
				menu.AddOption(new() { title = "退出游戏", description = "关闭游戏程序", });
				var choice = await menu.Start();
				if (choice == 0)
				{
					var game = new GameNode();
					AddChild(game);
					await game;
				}
				else
				{
					var dialogue = DialogueManager.CreateGenericDialogue();
					dialogue.SetText("玩家选择退出游戏 不出意外的话进程应该马上消失了");
					dialogue.Start();
					await dialogue;
					GetTree().Quit();
					return;
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
