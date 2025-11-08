using System;
using System.IO;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
using FileAccess = System.IO.FileAccess;
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
				var menu = DialogueManager.CreateMenuDialogue(
					new MenuOption { title = "开始游戏", description = "开始新的冒险", },
					new MenuOption { title = "读取游戏", description = "读取冒险", },
					new MenuOption { title = "退出游戏", description = "关闭游戏程序", }
				);
				var choice = await menu;
				const string file = "savegame.dat";
				switch (choice)
				{
					case 0:
					{
						var game = new GameNode(file);
						AddChild(game);
						await game;
						break;
					}
					case 1:
					{
						if (!File.Exists(file))
						{
							var dialogue = DialogueManager.CreateGenericDialogue();
							dialogue.SetText("未找到存档文件");
							await dialogue.StartTask();
							break;
						}
						await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
						using var reader = new BinaryReader(stream);
						var game = new GameNode(file, reader);
						AddChild(game);
						await game;
						break;
					}
					default:
					{
						var dialogue = DialogueManager.CreateGenericDialogue();
						dialogue.SetText("玩家选择退出游戏 不出意外的话进程应该马上消失了");
						await dialogue.StartTask();
						GetTree().Quit();
						return;
					}
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
