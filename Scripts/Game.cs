using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using FileAccess = System.IO.FileAccess;
public class Game
{
	static List<Character> CreateDefaultPlayers()
	{
		var hero = new Character("Ethan");
		if (hero.rightArm.Slots.Length > 0) hero.rightArm.Slots[0].Item = new LongSword();
		var raven = new Character("Raven");
		if (raven.rightArm.Slots.Length > 0) raven.rightArm.Slots[0].Item = new LongSword();
		return [hero, raven,];
	}
	static Character[] CreateDefaultEnemies()
	{
		var goblin = new Character("Goblin");
		if (goblin.rightArm.Slots.Length > 0) goblin.rightArm.Slots[0].Item = new LongSword();
		return [goblin,];
	}
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
	public int Chapter { get; private set; }
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
		this.saveFilePath = saveFilePath;
		this.gameNode = gameNode ?? throw new ArgumentNullException(nameof(gameNode));
		_ = new Snapshot(reader);
		players = ReadPlayers(reader);
		StartGameLoop();
	}
	public Snapshot GetSnapshot() => new(this);
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
				if (Chapter == 0)
				{
					using (DialogueManager.CreateGenericDialogue(out var dialogue))
					{
						await dialogue.ShowTextTask("年轻人");
						await dialogue.ShowTextTask("你一定没听过这个故事吧", "...");
						await dialogue.ShowTextTask("那是很久很久以前的事情了");
						await dialogue.ShowTextTask("就在我们这儿,有一名年轻的没落贵族");
						await dialogue.ShowTextTask("他从小失去了母亲");
						await dialogue.ShowTextTask("他的父亲成为政治斗争的牺牲品");
						await dialogue.ShowTextTask("他叫Ethan");
					}
					using (DialogueManager.CreateGenericDialogue(out var dialogue))
					{
						await dialogue.ShowTextTask("不久之后,私生子之战爆发了");
						await dialogue.ShowTextTask("可笑吧", "是", "否");
						await dialogue.ShowTextTask("但是老爷们总是乐此不疲");
						await dialogue.ShowTextTask("Ethan厌倦了政治,不想站队,于是选择了离开");
					}
				}
				var menu = DialogueManager.CreateMenuDialogue(
					"游戏菜单",
					new MenuOption { title = "开始战斗", description = "进入战斗场景", },
					new MenuOption { title = "装备", description = "管理角色装备与物品栏", },
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
						await ShowEquipmentFlow();
						break;
					}
					case 2:
					{
						await DialogueManager.ShowGenericDialogue("状态系统尚未实现");
						break;
					}
					case 3:
					{
						Save();
						await DialogueManager.ShowGenericDialogue("已保存当前进度");
						break;
					}
					case 4:
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
	/// <summary>
	///     装备交互主流程入口
	/// </summary>
	async Task ShowEquipmentFlow()
	{
		while (true)
		{
			var selected = await SelectCharacter();
			if (selected is null) return;
			var back = await SelectBodyPartAndExpand(selected);
			if (!back) return;
		}
	}
	/// <summary>
	///     选择角色
	/// </summary>
	async Task<Character?> SelectCharacter()
	{
		if (players.Count == 0)
		{
			await DialogueManager.ShowGenericDialogue("没有可用角色");
			return null;
		}
		var options = new MenuOption[players.Count];
		for (var i = 0; i < players.Count; i++)
		{
			var p = players[i];
			options[i] = new() { title = p.name, description = "选择该角色", };
		}
		var menu = DialogueManager.CreateMenuDialogue("选择角色", true, options);
		var choice = await menu;
		if (choice == options.Length) return null;
		return players[choice];
	}
	/// <summary>
	///     选择身体部位并展开容器
	/// </summary>
	async Task<bool> SelectBodyPartAndExpand(Character character)
	{
		while (true)
		{
			var bodyParts = character.bodyParts;
			var options = new MenuOption[bodyParts.Count];
			for (var i = 0; i < bodyParts.Count; i++)
			{
				var bp = bodyParts[i];
				var title = $"{bp.Name}";
				options[i] = new() { title = title, description = "查看或更换该部位装备", };
			}
			var menu = DialogueManager.CreateMenuDialogue("选择身体部位", true, options);
			var choice = await menu;
			if (choice == options.Length) return true;
			await ExpandItemContainer(character, bodyParts[choice], null);
		}
	}
	/// <summary>
	///     展开 IItemContainer：列出所有槽位，并在为装备时提供卸下
	/// </summary>
	async Task ExpandItemContainer(Character owner, IItemContainer container, ItemSlot? parentSlot)
	{
		while (true)
		{
			var slots = container.Slots;
			var dynamicOptions = new List<MenuOption>(slots.Length + 2);
			for (var i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				var title = slot.Item is null ? $"槽位{i + 1}: 空" : $"槽位{i + 1}: {slot.Item.Name}";
				var desc = slot.Item is null ? "选择以从物品栏换装" : "查看该装备";
				dynamicOptions.Add(new() { title = title, description = desc, });
			}
			var hasUnequip = container is Item && parentSlot != null && parentSlot.Item != null;
			if (hasUnequip) dynamicOptions.Add(new() { title = "卸下", description = "将此装备放入物品栏", });
			var menu = DialogueManager.CreateMenuDialogue("选择槽位", true, [.. dynamicOptions,]);
			var choice = await menu;
			if (choice == dynamicOptions.Count) return;
			if (hasUnequip && choice == dynamicOptions.Count - 1)
			{
				var item = parentSlot!.Item!;
				owner.inventory.Items.Add(item);
				parentSlot.Item = null;
				await DialogueManager.ShowGenericDialogue("已卸下装备并放入物品栏");
				return;
			}
			await ExpandItemSlot(owner, slots[choice]);
		}
	}
	/// <summary>
	///     展开 ItemSlot：空时从物品栏换装；有装备时进入其容器
	/// </summary>
	async Task ExpandItemSlot(Character owner, ItemSlot slot)
	{
		if (slot.Item is null)
			while (true)
			{
				if (owner.inventory.Items.Count == 0)
				{
					await DialogueManager.ShowGenericDialogue("物品栏为空");
					return;
				}
				var inv = owner.inventory.Items;
				var invOptions = new MenuOption[inv.Count];
				for (var i = 0; i < inv.Count; i++)
				{
					var it = inv[i];
					invOptions[i] = new() { title = it.Name, description = "换上该装备", };
				}
				var menu = DialogueManager.CreateMenuDialogue("选择装备", true, invOptions);
				var choice = await menu;
				if (choice == invOptions.Length) return;
				var candidate = inv[choice];
				try
				{
					slot.Item = candidate;
					inv.RemoveAt(choice);
					await DialogueManager.ShowGenericDialogue("已更换装备");
					return;
				}
				catch (ArgumentException)
				{
					await DialogueManager.ShowGenericDialogue("装备类型不匹配，无法更换");
				}
			}
		await ExpandItemContainer(owner, slot.Item, slot);
	}
}
