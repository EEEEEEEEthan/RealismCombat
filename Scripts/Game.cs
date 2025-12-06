using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using FileAccess = System.IO.FileAccess;
public enum ScriptCode
{
	_0_Intro = 0,
	_1_Equip = 1,
	_2_Wander = 2,
}
public class Game
{
	static List<Character> CreateDefaultPlayers()
	{
		var hero = new Character("Ethan");
		hero.inventory.Items.Add(Item.Create(ItemIdCode.LongSword));
		hero.inventory.Items.Add(Item.Create(ItemIdCode.ChainMail));
		hero.inventory.Items.Add(Item.Create(ItemIdCode.Belt));
		return [hero,];
	}
	static Character[] CreateDefaultEnemies()
	{
		var goblin = new Character("Goblin");
		if (goblin.rightArm.Slots.Length > 1) goblin.rightArm.Slots[1].Item = Item.Create(ItemIdCode.LongSword);
		return [goblin,];
	}
	static List<Character> ReadPlayers(BinaryReader reader)
	{
		var count = reader.ReadInt32();
		var result = new List<Character>();
		for (var i = 0; i < count; i++) result.Add(new(reader));
		return result;
	}
	readonly string saveFilePath;
	readonly TaskCompletionSource taskCompletionSource = new();
	readonly Node gameNode;
	readonly List<Character> players;
	public ScriptCode ScriptIndex { get; private set; }
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
		ScriptIndex = (ScriptCode)reader.ReadInt32();
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
		writer.Write((int)ScriptIndex);
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
			if (ScriptIndex == ScriptCode._0_Intro)
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
				ScriptIndex = ScriptCode._1_Equip;
			}
			if (ScriptIndex == ScriptCode._1_Equip)
			{
				while (true)
				{
					var readyForDeparture = players.Count > 0 &&
						HasEquippedItem(players[0], ItemIdCode.ChainMail) &&
						HasEquippedItem(players[0], ItemIdCode.LongSword);
					var choice = await DialogueManager.CreateMenuDialogue(
						"第一章 流浪",
						new MenuOption
						{
							title = "走吧...",
							description = readyForDeparture ? "离开这个鬼地方" : "你需要先装备好链甲和剑",
							disabled = !readyForDeparture,
						},
						new MenuOption { title = "装备", description = "管理角色装备与物品栏", },
						new MenuOption { title = "存档", description = "保存当前进度", },
						new MenuOption { title = "退出游戏", description = "返回主菜单", }
					);
					switch (choice)
					{
						case 0:
						{
							break;
						}
						case 1:
						{
							await ShowEquipmentFlow();
							break;
						}
						case 2:
						{
							Save();
							await DialogueManager.ShowGenericDialogue("已保存当前进度");
							break;
						}
						case 3:
						{
							Quit();
							return;
						}
					}
					if (players.Count > 0 &&
						HasEquippedItem(players[0], ItemIdCode.ChainMail) &&
						HasEquippedItem(players[0], ItemIdCode.LongSword))
						break;
				}
				ScriptIndex = ScriptCode._2_Wander;
			}
			if (ScriptIndex == ScriptCode._2_Wander)
			{
				using (DialogueManager.CreateGenericDialogue(out var dialogue))
				{
					await dialogue.ShowTextTask("Ethan穿上了父亲的旧盔甲,拿上了他的长剑");
					await dialogue.ShowTextTask("或许他的心里还存有一点家族荣誉的念想");
					await dialogue.ShowTextTask("走吧...");
				}
				using (DialogueManager.CreateGenericDialogue(out var dialogue))
				{
					await dialogue.ShowTextTask("战争时期的路上总是充满危险");
					await dialogue.ShowTextTask("Ethan四处躲避着巡逻的士兵");
					await dialogue.ShowTextTask("但还是被一个身穿华丽盔甲的男人发现了");
				}
				{
					int choice;
					using (DialogueManager.CreateGenericDialogue(out var dialogue))
					{
						await dialogue.ShowTextTask("\"停!\"那个男人大喝一声");
						await dialogue.ShowTextTask("或许还有一个好消息: 附近没有其他人");
						choice = await dialogue.ShowTextTask("Ethan开始紧张...", "上前交涉", "突袭!");
					}
					PackedScene combatNodeScene = ResourceTable.combatNodeScene;
					var combatNode = combatNodeScene.Instantiate<CombatNode>();
					gameNode.AddChild(combatNode);
					if (choice == 1)
						players[0].actionPoint.value = players[0].actionPoint.maxValue;
					else
						players[0].actionPoint.value = 0;
					var enemy = new Character("贵族兵");
					if (enemy.rightArm.Slots.Length > 1) enemy.rightArm.Slots[1].Item = Item.Create(ItemIdCode.LongSword);
					enemy.actionPoint.value = enemy.actionPoint.maxValue / 2;
					var enemies = new[]
					{
						enemy,
					};
					var combat = new Combat(players.ToArray(), enemies, combatNode);
					try
					{
						await combat;
					}
					finally
					{
						combatNode.QueueFree();
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
	/// <summary>
	///     在角色身上查找指定装备
	/// </summary>
	bool HasEquippedItem(Character character, ItemIdCode id)
	{
		foreach (var bodyPart in character.bodyParts)
			if (HasEquippedItem(bodyPart, id))
				return true;
		return false;
	}
	/// <summary>
	///     在容器及其子容器中查找指定装备
	/// </summary>
	bool HasEquippedItem(IItemContainer container, ItemIdCode id)
	{
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			if (item.id == id) return true;
			if (HasEquippedItem(item, id)) return true;
		}
		return false;
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
			var equippableParts = new List<BodyPart>();
			foreach (var bp in bodyParts)
			{
				if (bp.Slots.Length > 0) equippableParts.Add(bp);
			}
			if (equippableParts.Count == 0)
			{
				await DialogueManager.ShowGenericDialogue("没有可装备的部位");
				return true;
			}
			var options = new MenuOption[equippableParts.Count];
			for (var i = 0; i < equippableParts.Count; i++)
			{
				var bp = equippableParts[i];
				var title = bp.GetNameWithEquipments();
				options[i] = new() { title = title, description = "查看或更换该部位装备", };
			}
			var menu = DialogueManager.CreateMenuDialogue("选择身体部位", true, options);
			var choice = await menu;
			if (choice == options.Length) return true;
			await ExpandItemContainer(character, equippableParts[choice], null);
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
			var visibleSlots = new List<(ItemSlot Slot, int Index)>(slots.Length);
			for (var i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				if (!slot.VisibleInMenu) continue;
				visibleSlots.Add((slot, i));
			}
			var dynamicOptions = new List<MenuOption>(visibleSlots.Count + 2);
			foreach (var visibleSlot in visibleSlots)
			{
				var slot = visibleSlot.Slot;
				var titleIndex = visibleSlot.Index + 1;
				var title = slot.Item is null ? $"槽位{titleIndex}: 空" : $"槽位{titleIndex}: {slot.Item.Name}";
				var allowedDesc = $"可放入: {slot.Flag.GetDisplayName()}";
				var desc = slot.Item is null ? allowedDesc : FormatItemDescription(slot.Item);
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
			if (choice >= visibleSlots.Count) return;
			await ExpandItemSlot(owner, visibleSlots[choice].Slot);
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
				if (!TryBuildEquipOptions(inv, slot, out var invOptions, out var candidateIndices))
				{
					await DialogueManager.ShowGenericDialogue("没有适合该槽位的装备");
					return;
				}
				var menu = DialogueManager.CreateMenuDialogue("选择装备", true, invOptions);
				var choice = await menu;
				if (choice == invOptions.Length) return;
				var selectedInvIndex = candidateIndices[choice];
				var candidate = inv[selectedInvIndex];
				try
				{
					slot.Item = candidate;
					inv.RemoveAt(selectedInvIndex);
					return;
				}
				catch (ArgumentException)
				{
					await DialogueManager.ShowGenericDialogue("装备类型不匹配，无法更换");
				}
			}
		await ExpandItemContainer(owner, slot.Item, slot);
	}
	/// <summary>
	///     返回装备的展示描述: 首行显示flag, 次行显示原描述
	/// </summary>
	static string FormatItemDescription(Item item) => $"{item.flag.GetDisplayName()}\n{item.Description}";
	static bool CanEquip(Item item, ItemSlot slot) => (item.flag & slot.Flag) != 0;
	/// <summary>
	///     构建与槽位匹配的物品栏选项
	/// </summary>
	static bool TryBuildEquipOptions(List<Item> inventoryItems, ItemSlot slot, out MenuOption[] options, out List<int> indices)
	{
		var optionList = new List<MenuOption>();
		indices = new List<int>();
		for (var i = 0; i < inventoryItems.Count; i++)
		{
			var item = inventoryItems[i];
			if (!CanEquip(item, slot)) continue;
			indices.Add(i);
			optionList.Add(new MenuOption { title = item.Name, description = FormatItemDescription(item), });
		}
		options = optionList.ToArray();
		return options.Length > 0;
	}
}
