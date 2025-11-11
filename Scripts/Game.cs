using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
using RealismCombat.Combats;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Dialogues;
using RealismCombat.Nodes.Games;
using FileAccess = System.IO.FileAccess;
namespace RealismCombat;
public class Game
{
	public record Snapshot
	{
		readonly GameVersion version;
		readonly DateTime savedAt;
		public GameVersion Version => version;
		public DateTime SavedAt => savedAt;
		public string Title
		{
			get
			{
				var delta = DateTime.Now - SavedAt;
				return delta.TotalMinutes switch
				{
					< 1 => "刚刚",
					< 60 => $"{(int)delta.TotalMinutes}分钟前",
					_ => delta.TotalHours switch
					{
						< 24 => $"{(int)delta.TotalHours}小时前",
						_ => delta.TotalDays switch
						{
							< 7 => $"{(int)delta.TotalDays}天前",
							< 30 => $"{(int)(delta.TotalDays / 7)}周前",
							< 365 => $"{(int)(delta.TotalDays / 30)}个月前",
							_ => $"{SavedAt:yyyy-M-d}",
						},
					},
				};
			}
		}
		public string Desc => $"version: {version}";
		public Snapshot() : this(GameVersion.newest, DateTime.UtcNow) { }
		public Snapshot(BinaryReader reader)
		{
			using (reader.ReadScope())
			{
				version = new(reader);
				savedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc).ToLocalTime();
			}
		}
		Snapshot(GameVersion version, DateTime savedAt)
		{
			this.version = version;
			this.savedAt = savedAt;
		}
		public void Serialize(BinaryWriter writer)
		{
			using (writer.WriteScope())
			{
				version.Serialize(writer);
				writer.Write(savedAt.ToUniversalTime().Ticks);
			}
		}
	}
	static List<Character> CreateDefaultPlayers() => [new("Hero"),];
	static Character[] CreateDefaultEnemies() => [new("Goblin"),];
	static List<Character> ReadPlayers(BinaryReader reader)
	{
		var count = reader.ReadInt32();
		var result = new List<Character>(count);
		for (var i = 0; i < count; i++) result.Add(new(reader));
		return result;
	}
	readonly string saveFilePath;
	readonly TaskCompletionSource taskCompletionSource = new();
	readonly Node gameNode;
	readonly List<Character> players;
	/// <summary>
	///     新游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	/// <param name="gameNode">用于承载场景的节点</param>
	public Game(string saveFilePath, Node gameNode)
	{
		this.saveFilePath = saveFilePath;
		this.gameNode = gameNode;
		players = CreateDefaultPlayers();
		StartGameLoop();
	}
	/// <summary>
	///     读取游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	/// <param name="reader"></param>
	/// <param name="gameNode">用于承载场景的节点</param>
	public Game(string saveFilePath, BinaryReader reader, Node gameNode)
	{
		if (gameNode is null) throw new ArgumentNullException(nameof(gameNode));
		this.saveFilePath = saveFilePath;
		this.gameNode = gameNode;
		_ = new Snapshot(reader);
		players = ReadPlayers(reader);
		StartGameLoop();
	}
	public Snapshot GetSnapshot() => new();
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	void Save()
	{
		if (string.IsNullOrEmpty(saveFilePath)) return;
		var directory = Path.GetDirectoryName(saveFilePath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
		using var stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new BinaryWriter(stream);
		var snapshot = GetSnapshot();
		snapshot.Serialize(writer);
		WritePlayers(writer);
	}
	void Quit()
	{
		Save();
		taskCompletionSource.SetResult();
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
					new MenuOption { title = "存档", description = "保存当前进度", },
					new MenuOption { title = "退出游戏", description = "返回主菜单", }
				);
				var choice = await menu;
				switch (choice)
				{
					case 0:
					{
						PackedScene combatNodeScene = ResourceTable.combatNodeScene;
						var combatNode = combatNodeScene.Instantiate<CombatNode>();
						gameNode.AddChild(combatNode);
						var combat = new Combat(players.ToArray(), CreateDefaultEnemies(), combatNode);
						try
						{
							await combat;
						}
						finally
						{
							combatNode.QueueFree();
						}
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
						Save();
						var dialogue = DialogueManager.CreateGenericDialogue("已保存当前进度");
						await dialogue;
						break;
					}
					case 3:
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
	void WritePlayers(BinaryWriter writer)
	{
		writer.Write(players.Count);
		foreach (var player in players) player.Serialize(writer);
	}
}
