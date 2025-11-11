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
		public Snapshot() => version = GameVersion.newest;
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
