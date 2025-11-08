using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
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
				var menu = DialogueManager.CreateMenuDialogue();
				menu.AddOption(new() { title = "开始战斗", description = "进入战斗场景", });
				menu.AddOption(new() { title = "查看状态", description = "查看角色状态", });
				menu.AddOption(new() { title = "退出游戏", description = "返回主菜单", });
				var choice = await menu.StartTask();
				switch (choice)
				{
					case 0:
					{
						var dialogue = DialogueManager.CreateGenericDialogue();
						dialogue.SetText("战斗系统尚未实现");
						await dialogue.StartTask();
						break;
					}
					case 1:
					{
						var dialogue = DialogueManager.CreateGenericDialogue();
						dialogue.SetText("状态系统尚未实现");
						await dialogue.StartTask();
						break;
					}
					case 2:
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
