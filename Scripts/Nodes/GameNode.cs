using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
using FileAccess = System.IO.FileAccess;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	public record Snapshot
	{
		readonly GameVersion version;
		public Snapshot(GameNode game) => version = GameVersion.newest;
		public Snapshot(BinaryReader reader)
		{
			using (reader.ReadScope())
			{
				version = new(reader);
			}
		}
		public void Serialize(BinaryWriter writer)
		{
			using (writer.WriteScope())
			{
				version.Serialize(writer);
			}
		}
	}
	readonly string saveFilePath;
	TaskCompletionSource? taskCompletionSource;
	/// <summary>
	///     新游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	public GameNode(string saveFilePath)
	{
		this.saveFilePath = saveFilePath;
		StartGameLoop();
	}
	/// <summary>
	///     读取游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	/// <param name="reader"></param>
	public GameNode(string saveFilePath, BinaryReader reader)
	{
		this.saveFilePath = saveFilePath;
		_ = new Snapshot(reader);
		StartGameLoop();
	}
	public Snapshot GetSnapshot() => new(this);
	public TaskAwaiter GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	void Save()
	{
		using var stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new BinaryWriter(stream);
		var snapshot = GetSnapshot();
		snapshot.Serialize(writer);
	}
	void Quit()
	{
		taskCompletionSource?.SetResult();
		QueueFree();
	}
	async void StartGameLoop()
	{
		try
		{
			while (true)
			{
				var menu = DialogueManager.CreateMenuDialogue(
					new MenuOption { title = "开始战斗", description = "进入战斗场景", },
					new MenuOption { title = "查看状态", description = "查看角色状态", },
					new MenuOption { title = "技能配置", description = "调整战斗中可用技能", },
					new MenuOption { title = "背包", description = "管理携带物品", },
					new MenuOption { title = "任务日志", description = "查看当前任务进度", },
					new MenuOption { title = "保存进度", description = "保存当前游戏状态", },
					new MenuOption { title = "系统设置", description = "调整游戏内设置", },
					new MenuOption { title = "退出游戏", description = "返回主菜单", }
				);
				var choice = await menu;
				switch (choice)
				{
					case 0:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("战斗系统尚未实现");
						await dialogue;
						break;
					}
					case 1:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("状态系统尚未实现");
						await dialogue;
						break;
					}
					case 2:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("技能配置功能尚未实现");
						await dialogue;
						break;
					}
					case 3:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("背包系统尚未实现");
						await dialogue;
						break;
					}
					case 4:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("任务日志尚未实现");
						await dialogue;
						break;
					}
					case 5:
					{
						Save();
						var dialogue = DialogueManager.CreateGenericDialogue("进度已保存");
						await dialogue;
						break;
					}
					case 6:
					{
						var dialogue = DialogueManager.CreateGenericDialogue("系统设置尚未实现");
						await dialogue;
						break;
					}
					case 7:
					{
						Quit();
						return;
					}
					default:
					{
						throw new InvalidOperationException($"未知的菜单选项: {choice}");
					}
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			Quit();
		}
	}
}
