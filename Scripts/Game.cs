using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RealismCombat.AutoLoad;
using RealismCombat.Combats;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
using FileAccess = System.IO.FileAccess;
namespace RealismCombat.Nodes;
public class Game
{
	public record Snapshot
	{
		readonly GameVersion version;
		public Snapshot(Game game) => version = GameVersion.newest;
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
	readonly TaskCompletionSource taskCompletionSource = new();
	/// <summary>
	///     新游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	public Game(string saveFilePath) : this()
	{
		this.saveFilePath = saveFilePath;
		StartGameLoop();
	}
	/// <summary>
	///     读取游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	/// <param name="reader"></param>
	public Game(string saveFilePath, BinaryReader reader) : this()
	{
		this.saveFilePath = saveFilePath;
		_ = new Snapshot(reader);
		StartGameLoop();
	}
	Game() { }
	public Snapshot GetSnapshot() => new(this);
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	void Save()
	{
		using var stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new BinaryWriter(stream);
		var snapshot = GetSnapshot();
		snapshot.Serialize(writer);
	}
	void Quit() => taskCompletionSource?.SetResult();
	async void StartGameLoop()
	{
		try
		{
			while (true)
			{
				var menu = DialogueManager.CreateMenuDialogue(
					new MenuOption { title = "开始战斗", description = "进入战斗场景", },
					new MenuOption { title = "查看状态", description = "查看角色状态", },
					new MenuOption { title = "退出游戏", description = "返回主菜单", }
				);
				var choice = await menu;
				switch (choice)
				{
					case 0:
					{
						var combat = new Combat(
							[new("Hero"),],
							[new("Goblin"),]
						);
						await combat;
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
