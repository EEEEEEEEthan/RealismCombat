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
	_3_ToBeContinued = 3,
}
public class Game
{
	static List<Character> ReadPlayers(BinaryReader reader, GameVersion version)
	{
		var count = reader.ReadInt32();
		var result = new List<Character>();
		for (var i = 0; i < count; i++) result.Add(new(reader));
		return result;
	}
	/// <summary>
	///     返回装备的展示描述: 首行显示flag, 次行显示原描述
	/// </summary>
	static string FormatItemDescription(Item item) => $"{item.flag.GetDisplayName()}\n{item.Description}";
	/// <summary>
	///     获取槽位标题: 空为#空, 有装备时附带其子装备
	/// </summary>
	static string FormatSlotTitle(ItemSlot slot)
	{
		var item = slot.Item;
		if (item == null) return "#空";
		var parts = new List<string> { $"#{item.Name}", };
		foreach (var childSlot in item.Slots)
		{
			var childItem = childSlot.Item;
			if (childItem == null) continue;
			parts.Add(childItem.IconTag);
		}
		return string.Concat(parts);
	}
	static bool CanEquip(Item item, ItemSlot slot) => (item.flag & slot.Flag) != 0;
	/// <summary>
	///     构建与槽位匹配的物品栏选项
	/// </summary>
	static bool TryBuildEquipOptions(List<Item> inventoryItems, ItemSlot slot, out MenuOption[] options, out List<int> indices)
	{
		var optionList = new List<MenuOption>();
		indices = [];
		for (var i = 0; i < inventoryItems.Count; i++)
		{
			var item = inventoryItems[i];
			if (!CanEquip(item, slot)) continue;
			indices.Add(i);
			optionList.Add(new() { title = $"{item.IconTag}{item.Name}", description = FormatItemDescription(item), });
		}
		options = optionList.ToArray();
		return options.Length > 0;
	}
	readonly string saveFilePath;
	readonly TaskCompletionSource taskCompletionSource = new();
	readonly Node gameNode;
	readonly List<Character> players;
	public ScriptCode ScriptIndex { get; private set; }
	Snapshot Snapshot => new(this);
	/// <summary>
	///     新游戏
	/// </summary>
	/// <param name="saveFilePath"></param>
	/// <param name="gameNode">用于承载场景的节点</param>
	public Game(string saveFilePath, Node gameNode)
	{
		this.saveFilePath = saveFilePath;
		this.gameNode = gameNode;
		var hero = new Character("Ethan");
		var longSword = Item.Create(ItemIdCode.LongSword);
		var cottonLiner = Item.Create(ItemIdCode.CottonLiner);
		var belt = Item.Create(ItemIdCode.Belt);
		var cottonPants = Item.Create(ItemIdCode.CottonPants);
		hero.inventory.Items.Add(longSword);
		if (hero.torso.Slots.Length > 0) hero.torso.Slots[0].Item = cottonLiner;
		if (hero.torso.Slots.Length > 1) hero.torso.Slots[1].Item = belt;
		if (hero.groin.Slots.Length > 0) hero.groin.Slots[0].Item = cottonPants;
		hero.availableCombatActions.Clear();
		hero.availableCombatActions[CombatActionCode.Slash] = 0f;
		hero.availableCombatActions[CombatActionCode.Stab] = 0f;
		hero.availableCombatActions[CombatActionCode.Grab] = 0f;
		hero.availableCombatActions[CombatActionCode.BreakFree] = 0f;
		hero.availableCombatActions[CombatActionCode.Release] = 0f;
		hero.availableCombatActions[CombatActionCode.TakeWeapon] = 0f;
		hero.availableCombatActions[CombatActionCode.PickWeapon] = 0f;
#if DEBUG
		hero.availableCombatActions[CombatActionCode.Execution] = 0f;
#endif
		players = [hero,];
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
		var snapshot = new Snapshot(reader);
		var version = snapshot.Version;
		players = ReadPlayers(reader, version);
		ScriptIndex = (ScriptCode)reader.ReadInt32();
#if DEBUG
		foreach (var player in players)
			if (!player.availableCombatActions.ContainsKey(CombatActionCode.Execution))
				player.availableCombatActions[CombatActionCode.Execution] = 0f;
#endif
		StartGameLoop();
	}
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	void Save()
	{
		if (string.IsNullOrEmpty(saveFilePath)) return;
		var directory = Path.GetDirectoryName(saveFilePath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
		using var stream = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
		using var writer = new BinaryWriter(stream);
		var snapshot = Snapshot;
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
			PlayStoryBgm();
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
						HasEquippedItem(players[0], ItemIdCode.LongSword);
					var proceed = false;
					var choice = await DialogueManager.CreateMenuDialogue(
						"第一章 流浪",
						new MenuOption
						{
							title = "走吧...",
							description = readyForDeparture ? "离开这个鬼地方" : "你需要先装备好长剑",
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
							proceed = readyForDeparture && HasEquippedItem(players[0], ItemIdCode.LongSword);
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
					if (proceed) break;
				}
				ScriptIndex = ScriptCode._2_Wander;
			}
			if (ScriptIndex == ScriptCode._2_Wander)
			{
				using (DialogueManager.CreateGenericDialogue(out var dialogue))
				{
					await dialogue.ShowTextTask("Ethan拿上了父亲的长剑");
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
					var player = players[0];
					Item? weapon = null;
					using (DialogueManager.CreateGenericDialogue(out var dialogue))
					{
						await dialogue.ShowTextTask("\"停!\"那个男人大喝一声");
						await dialogue.ShowTextTask("或许还有一个好消息: 附近没有其他人");
						var beltWeaponCandidates = player.BeltWeaponCandidates;
						var emptyHandSlot = player.EmptyHandSlot;
						var optionList = new List<string> { "上前交涉", };
						if (emptyHandSlot != null && beltWeaponCandidates.Count > 0)
							foreach (var candidate in beltWeaponCandidates)
							{
								var weaponName = candidate.Slot.Item?.Name ?? "武器";
								optionList.Add($"抽出{weaponName}");
							}
						var choice = await dialogue.ShowTextTask("Ethan开始紧张...", optionList.ToArray());
						if (choice > 0 && emptyHandSlot != null && beltWeaponCandidates.Count >= choice)
						{
							var selected = beltWeaponCandidates[choice - 1];
							weapon = selected.Slot.Item;
							if (weapon != null)
							{
								emptyHandSlot.Value.slot.Item = weapon;
								selected.Slot.Item = null;
							}
						}
					}
					if (weapon == null)
						using (DialogueManager.CreateGenericDialogue(out var dialogue))
						{
							await dialogue.ShowTextTask("Ethan试图与那个男人交涉", "自我介绍", "你是谁");
							await dialogue.ShowTextTask("男人什么也没说,右手摸向了腰间的剑柄...");
						}
					else
						using (DialogueManager.CreateGenericDialogue(out var dialogue))
						{
							await dialogue.ShowTextTask($"Ethan迅速抽出{weapon.Name}");
						}
					PackedScene combatNodeScene = ResourceTable.combatNodeScene;
					var combatNode = combatNodeScene.Instantiate<CombatNode>();
					gameNode.AddChild(combatNode);
					player.actionPoint.value = weapon == null
						? player.actionPoint.maxValue / 2
						: player.actionPoint.maxValue;
					var enemy = new Character("贵族兵");
					if (enemy.rightArm.Slots.Length > 1) enemy.rightArm.Slots[1].Item = Item.Create(ItemIdCode.LongSword);
					enemy.actionPoint.value = 7;
					enemy.availableCombatActions.Clear();
					enemy.availableCombatActions[CombatActionCode.Slash] = 0f;
					enemy.availableCombatActions[CombatActionCode.BreakFree] = 0f;
					enemy.availableCombatActions[CombatActionCode.Release] = 0f;
					enemy.availableCombatActions[CombatActionCode.TakeWeapon] = 0f;
					enemy.availableCombatActions[CombatActionCode.Charge] = 0f;
					player.availableCombatActions[CombatActionCode.Charge] = 0f;
					var enemies = new[]
					{
						enemy,
					};
					AudioManager.PlayBgm(ResourceTable.battleMusic1);
					var combat = new Combat(players.ToArray(), enemies, combatNode);
					try
					{
						await combat;
					}
					finally
					{
						PlayStoryBgm();
						combatNode.QueueFree();
					}
					ScriptIndex = ScriptCode._3_ToBeContinued;
				}
			}
			if (ScriptIndex == ScriptCode._3_ToBeContinued)
			{
				using (DialogueManager.CreateGenericDialogue(out var dialogue))
				{
					await dialogue.ShowTextTask("Ethan杀死了贵族兵");
					await dialogue.ShowTextTask("虽然Ethan从小接受战斗训练,但这毕竟是他第一次真正面对生死");
					await dialogue.ShowTextTask("他的心跳得飞快,手也在颤抖");
					await dialogue.ShowTextTask("他一头扎进树林");
				}
				using (DialogueManager.CreateGenericDialogue(out var dialogue))
				{
					await dialogue.ShowTextTask("后来啊");
					await dialogue.ShowTextTask("后来的故事,你去玩<醉酒的Ethan>吧");
				}
				taskCompletionSource.SetResult();
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			Quit();
		}
	}
	void PlayStoryBgm() => AudioManager.PlayBgm(ResourceTable.arpegio01Loop);
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
			if (players.Count == 1) return;
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
		if (players.Count == 1) return players[0];
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
				if (bp.Slots.Length > 0)
					equippableParts.Add(bp);
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
			var menu = DialogueManager.CreateMenuDialogue($"{character.name}的装备", true, options);
			var choice = await menu;
			if (choice == options.Length) return true;
			var navigationTitle = $"{character.name}>{equippableParts[choice].Name}";
			await ExpandItemContainer(character, equippableParts[choice], null, navigationTitle);
		}
	}
	/// <summary>
	///     展开 IItemContainer：列出所有槽位，并在为装备时提供卸下
	/// </summary>
	async Task ExpandItemContainer(Character owner, IItemContainer container, ItemSlot? parentSlot, string navigationTitle)
	{
		while (true)
		{
			var slots = container.Slots;
			var visibleSlots = new List<ItemSlot>(slots.Length);
			for (var i = 0; i < slots.Length; i++)
			{
				var slot = slots[i];
				if (!slot.VisibleInMenu) continue;
				visibleSlots.Add(slot);
			}
			var dynamicOptions = new List<MenuOption>(visibleSlots.Count + 2);
			foreach (var visibleSlot in visibleSlots)
			{
				var slot = visibleSlot;
				var title = FormatSlotTitle(slot);
				var allowedDesc = $"可放入: {slot.Flag.GetDisplayName()}";
				var desc = slot.Item is null ? allowedDesc : FormatItemDescription(slot.Item);
				dynamicOptions.Add(new() { title = title, description = desc, });
			}
			var hasUnequip = container is Item && parentSlot is { Item: not null, };
			if (hasUnequip) dynamicOptions.Add(new() { title = "卸下", description = "将此装备放入物品栏", });
			var menu = DialogueManager.CreateMenuDialogue(navigationTitle, true, [.. dynamicOptions,]);
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
			var slotNavigationTitle = $"{navigationTitle}>{visibleSlots[choice].Item?.Name ?? visibleSlots[choice].Flag.GetDisplayName()}";
			await ExpandItemSlot(owner, visibleSlots[choice], slotNavigationTitle);
		}
	}
	/// <summary>
	///     展开 ItemSlot：空时从物品栏换装；有装备时进入其容器
	/// </summary>
	async Task ExpandItemSlot(Character owner, ItemSlot slot, string navigationTitle)
	{
		if (slot.Item is null)
			while (true)
			{
				if (owner.inventory.Items.Count == 0)
				{
					await DialogueManager.ShowGenericDialogue("物品栏为空");
					return;
				}
				var equipNavigationTitle = $"{navigationTitle}>选择装备";
				var inv = owner.inventory.Items;
				if (!TryBuildEquipOptions(inv, slot, out var invOptions, out var candidateIndices))
				{
					await DialogueManager.ShowGenericDialogue("没有适合该槽位的装备");
					return;
				}
				var menu = DialogueManager.CreateMenuDialogue(equipNavigationTitle, true, invOptions);
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
		await ExpandItemContainer(owner, slot.Item, slot, navigationTitle);
	}
}
