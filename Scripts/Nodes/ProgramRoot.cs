using System;
using System.IO;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
using FileAccess = System.IO.FileAccess;
namespace RealismCombat.Nodes;
/// <summary>
///     程序根节点，负责初始化游戏生命周期
/// </summary>
public partial class ProgramRoot : Node
{
	public override void _Ready()
	{
		Log.Print("[ProgramRoot] 程序启动");
		var godotPath = Settings.Get("godot");
		if (godotPath != null) Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
		if (LaunchArgs.port.HasValue)
			AddChild(new CommandHandler(this));
		else
			StartGameLoop();
	}
	public async void StartGameLoop()
	{
		try
		{
			while (this.Valid())
				try
				{
					if (!await Routine()) break;
				}
				catch (Exception e)
				{
					Log.PrintException(e);
				}
			await Task.Delay(1);
			GetTree().Quit();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
	async Task<bool> Routine()
	{
		var menu = DialogueManager.CreateMenuDialogue(
			new MenuOption { title = "开始游戏", description = "开始新的冒险", },
			new MenuOption { title = "读取游戏", description = "读取冒险", },
			new MenuOption { title = "退出游戏", description = "关闭游戏程序", }
		);
		var choice = await menu;
		const string file = "save.dat";
		switch (choice)
		{
			case 0:
			{
				var game = new Game(file);
				await game;
				break;
			}
			case 1:
			{
				if (!File.Exists(file))
				{
					var dialogue = DialogueManager.CreateGenericDialogue("未找到存档文件");
					await dialogue;
					break;
				}
				await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read);
				using var reader = new BinaryReader(stream);
				var game = new Game(file, reader);
				await game;
				break;
			}
			case 2:
			{
				Log.Print("游戏即将退出...");
				GameServer.McpCheckpoint();
				await Task.Delay(1);
				GetTree().Quit();
				return false;
			}
			default:
			{
				throw new InvalidOperationException("未知的菜单选项");
			}
		}
		return true;
	}
}
