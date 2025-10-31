using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using RealismCombat.Data;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	public abstract class State(GameNode gameNode)
	{
		public static State Create(GameNode gameNode)
		{
			if (gameNode.gameData.state == CombatState.serializeId && gameNode.gameData.combatData == null)
			{
				return new IdleState(gameNode);
			}
			return gameNode.gameData.state switch
			{
				IdleState.serializeId => new IdleState(gameNode),
				CombatState.serializeId => new CombatState(gameNode: gameNode,
					combatData: gameNode.gameData.combatData ?? throw new InvalidOperationException("战斗数据为空")),
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
		public readonly GameNode gameNode = gameNode;
		public Action? OnExit;
	}
	public class IdleState : State
	{
		public const int serializeId = 0;
		public MenuDialogue? dialogue;
		public IdleState(GameNode gameNode) : base(gameNode)
		{
			gameNode.CurrentState = this;
			gameNode.Root.PlayMusic(AudioTable.arpegio01Loop45094);
			dialogue = gameNode.Root.CreateDialogue();
			dialogue.Initialize(new()
			{
				title = "游戏菜单",
				options =
				[
					new()
					{
						option = "进入战斗",
						description = "开始战斗",
						onPreview = () => { },
						onConfirm = () =>
						{
							dialogue?.QueueFree();
							var combatData = new CombatData();
							var (min, max) = CharacterData.InitialActionPointRange;
							combatData.characters.Add(new(name: "ethan", team: 0) { ActionPoint = GD.Randf() * (max - min) + min, });
							combatData.characters.Add(new(name: "dove", team: 1) { ActionPoint = GD.Randf() * (max - min) + min, });
							_ = new CombatState(gameNode: gameNode, combatData: combatData);
						},
						available = true,
					},
					new()
					{
						option = "物品栏",
						description = "查看物品",
						onPreview = () => { },
						onConfirm = () =>
						{
							ShowInventoryMenu(gameNode: gameNode);
						},
						available = true,
					},
					new()
					{
						option = "装备",
						description = "管理装备",
						onPreview = () => { },
						onConfirm = () =>
						{
							ShowEquipmentMenu(gameNode: gameNode);
						},
						available = true,
					},
					new()
					{
						option = "退出",
						description = "返回主菜单",
						onPreview = () => { },
						onConfirm = () =>
						{
							dialogue?.QueueFree();
							gameNode.QueueFree();
							gameNode.Root.state = new ProgramRootNode.IdleState(gameNode.Root);
						},
						available = true,
					},
				],
			});
			OnExit = () =>
			{
				dialogue?.QueueFree();
			};
		}
		static CharacterData GetOrCreatePlayerCharacter(GameNode gameNode)
		{
			if (gameNode.gameData.combatData != null)
			{
				foreach (var character in gameNode.gameData.combatData.characters)
				{
					if (character.PlayerControlled)
					{
						return character;
					}
				}
			}
			if (gameNode.gameData.combatData == null)
			{
				gameNode.gameData.combatData = new CombatData();
				gameNode.Save();
			}
			var playerCharacter = new CharacterData(name: "ethan", team: 0);
			gameNode.gameData.combatData.characters.Add(playerCharacter);
			return playerCharacter;
		}
		static string GetItemName(uint itemId)
		{
			return ItemConfig.Configs.TryGetValue(itemId, out var config) ? config.name : $"物品{itemId}";
		}
		static bool CanStack(ItemData item1, ItemData item2)
		{
			if (item1.itemId != item2.itemId) return false;
			if (item1.slots.Length != item2.slots.Length) return false;
			return item1.slots.All(slot => slot == null) && item2.slots.All(slot => slot == null);
		}
		static void AddItemToInventory(GameNode gameNode, ItemData item)
		{
			if (item.slots.Length > 0 && item.slots.Any(slot => slot != null))
			{
				gameNode.gameData.items.Add(item);
				return;
			}
			foreach (var existingItem in gameNode.gameData.items)
			{
				if (CanStack(existingItem, item))
				{
					existingItem.count += item.count;
					return;
				}
			}
			gameNode.gameData.items.Add(item);
		}
		static void ShowInventoryMenu(GameNode gameNode)
		{
			var inventoryDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			if (gameNode.gameData.items.Count == 0)
			{
				options.Add(new()
				{
					option = "物品栏为空",
					description = null,
					onPreview = () => { },
					onConfirm = () => { },
					available = false,
				});
			}
			else
			{
				foreach (var item in gameNode.gameData.items)
				{
					var itemName = GetItemName(item.itemId);
					options.Add(new()
					{
						option = $"{itemName} x{item.count}",
						description = null,
						onPreview = () => { },
						onConfirm = () => { },
						available = false,
					});
				}
			}
			inventoryDialogue.Initialize(new()
			{
				title = "物品栏",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回游戏菜单");
		}
		static void ShowEquipmentMenu(GameNode gameNode)
		{
			var character = GetOrCreatePlayerCharacter(gameNode);
			var equipmentDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			foreach (var bodyPart in character.bodyParts)
			{
				var partName = bodyPart.id.GetName();
				var optionText = bodyPart.slot == null ? partName : $"{partName}[{GetItemName(bodyPart.slot.itemId)}]";
				options.Add(new()
				{
					option = optionText,
					description = null,
					onPreview = () => { },
					onConfirm = () =>
					{
						ShowSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: equipmentDialogue);
					},
					available = true,
				});
			}
			equipmentDialogue.Initialize(new()
			{
				title = "装备",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回游戏菜单");
		}
		static void ShowSlotMenu(GameNode gameNode, CharacterData character, BodyPartData bodyPart, MenuDialogue parentDialogue)
		{
			if (bodyPart.slot == null)
			{
				ShowEmptySlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: parentDialogue);
			}
			else
			{
				ShowEquippedSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: parentDialogue, item: bodyPart.slot);
			}
		}
		static void ShowEmptySlotMenu(GameNode gameNode, CharacterData character, BodyPartData bodyPart, MenuDialogue parentDialogue)
		{
			var slotDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			if (gameNode.gameData.items.Count == 0)
			{
				options.Add(new()
				{
					option = "没有可装备的物品",
					description = null,
					onPreview = () => { },
					onConfirm = () => { },
					available = false,
				});
			}
			else
			{
				foreach (var item in gameNode.gameData.items)
				{
					var itemId = item.itemId;
					var itemName = GetItemName(itemId);
					options.Add(new()
					{
						option = $"{itemName} x{item.count}",
						description = null,
						onPreview = () => { },
						onConfirm = () =>
						{
							bodyPart.slot = new ItemData(itemId: itemId, count: 1);
							if (item.count > 1)
							{
								item.count--;
							}
							else
							{
								gameNode.gameData.items.Remove(item);
							}
							gameNode.Save();
							slotDialogue?.QueueFree();
							parentDialogue?.QueueFree();
						},
						available = true,
					});
				}
			}
			slotDialogue.Initialize(new()
			{
				title = $"{bodyPart.id.GetName()} - 选择物品",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回装备菜单");
		}
		static void ShowEquippedSlotMenu(GameNode gameNode, CharacterData character, BodyPartData bodyPart, MenuDialogue parentDialogue, ItemData item)
		{
			var slotDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			if (item.slots.Length > 0)
			{
				for (var i = 0; i < item.slots.Length; i++)
				{
					var slotIndex = i;
					var slotItem = item.slots[i];
					var optionText = slotItem == null ? $"槽位{slotIndex + 1}(空)" : $"槽位{slotIndex + 1}[{GetItemName(slotItem.itemId)}]";
					options.Add(new()
					{
						option = optionText,
						description = null,
						onPreview = () => { },
						onConfirm = () =>
						{
							if (slotItem == null)
							{
								ShowEmptyItemSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: parentDialogue, item: item, slotIndex: slotIndex, slotDialogue: slotDialogue);
							}
							else
							{
								ShowEquippedItemSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: parentDialogue, item: item, slotIndex: slotIndex, slotDialogue: slotDialogue, slotItem: slotItem);
							}
						},
						available = true,
					});
				}
			}
			var hasNonEmptySlot = item.slots.Length > 0 && item.slots.Any(slot => slot != null);
			if (!hasNonEmptySlot)
			{
				options.Add(new()
				{
					option = "卸下",
					description = "卸下这个装备",
					onPreview = () => { },
					onConfirm = () =>
					{
						if (bodyPart.slot != null)
						{
							AddItemToInventory(gameNode: gameNode, item: bodyPart.slot);
							bodyPart.slot = null;
							gameNode.Save();
						}
						slotDialogue?.QueueFree();
						parentDialogue?.QueueFree();
					},
					available = true,
				});
			}
			slotDialogue.Initialize(new()
			{
				title = $"{bodyPart.id.GetName()} - {GetItemName(item.itemId)}",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回上级菜单");
		}
		static void ShowEmptyItemSlotMenu(GameNode gameNode, CharacterData character, BodyPartData bodyPart, MenuDialogue parentDialogue, ItemData item, int slotIndex, MenuDialogue slotDialogue)
		{
			var itemSlotDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			if (gameNode.gameData.items.Count == 0)
			{
				options.Add(new()
				{
					option = "没有可装备的物品",
					description = null,
					onPreview = () => { },
					onConfirm = () => { },
					available = false,
				});
			}
			else
			{
				foreach (var inventoryItem in gameNode.gameData.items)
				{
					var itemId = inventoryItem.itemId;
					var itemName = GetItemName(itemId);
					options.Add(new()
					{
						option = $"{itemName} x{inventoryItem.count}",
						description = null,
						onPreview = () => { },
						onConfirm = () =>
						{
							item.slots[slotIndex] = new ItemData(itemId: itemId, count: 1);
							if (inventoryItem.count > 1)
							{
								inventoryItem.count--;
							}
							else
							{
								gameNode.gameData.items.Remove(inventoryItem);
							}
							gameNode.Save();
							itemSlotDialogue?.QueueFree();
							slotDialogue?.QueueFree();
						},
						available = true,
					});
				}
			}
			itemSlotDialogue.Initialize(new()
			{
				title = $"槽位{slotIndex + 1} - 选择物品",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回上级菜单");
		}
		static void ShowEquippedItemSlotMenu(GameNode gameNode, CharacterData character, BodyPartData bodyPart, MenuDialogue parentDialogue, ItemData item, int slotIndex, MenuDialogue slotDialogue, ItemData slotItem)
		{
			var itemSlotDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			if (slotItem.slots.Length > 0)
			{
				for (var i = 0; i < slotItem.slots.Length; i++)
				{
					var nestedSlotIndex = i;
					var nestedSlotItem = slotItem.slots[i];
					var optionText = nestedSlotItem == null ? $"槽位{nestedSlotIndex + 1}(空)" : $"槽位{nestedSlotIndex + 1}[{GetItemName(nestedSlotItem.itemId)}]";
					options.Add(new()
					{
						option = optionText,
						description = null,
						onPreview = () => { },
						onConfirm = () =>
						{
							if (nestedSlotItem == null)
							{
								ShowEmptyItemSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: slotDialogue, item: slotItem, slotIndex: nestedSlotIndex, slotDialogue: itemSlotDialogue);
							}
							else
							{
								ShowEquippedItemSlotMenu(gameNode: gameNode, character: character, bodyPart: bodyPart, parentDialogue: slotDialogue, item: slotItem, slotIndex: nestedSlotIndex, slotDialogue: itemSlotDialogue, slotItem: nestedSlotItem);
							}
						},
						available = true,
					});
				}
			}
			var hasNonEmptySlot = slotItem.slots.Length > 0 && slotItem.slots.Any(slot => slot != null);
			if (!hasNonEmptySlot)
			{
				options.Add(new()
				{
					option = "卸下",
					description = "卸下这个装备",
					onPreview = () => { },
					onConfirm = () =>
					{
						AddItemToInventory(gameNode: gameNode, item: slotItem);
						item.slots[slotIndex] = null;
						gameNode.Save();
						itemSlotDialogue?.QueueFree();
						slotDialogue?.QueueFree();
						parentDialogue?.QueueFree();
					},
					available = true,
				});
			}
			itemSlotDialogue.Initialize(new()
			{
				title = $"槽位{slotIndex + 1} - {GetItemName(slotItem.itemId)}",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回上级菜单");
		}
	}
	public class CombatState : State
	{
		public const int serializeId = 1;
		public CombatState(GameNode gameNode, CombatData combatData) : base(gameNode)
		{
			gameNode.CurrentState = this;
			gameNode.Root.PlayMusic(AudioTable.battleMusic1);
			gameNode.gameData.combatData = combatData;
			gameNode.Save();
			var combatNode = CombatNode.Create(gameNode: gameNode, combatData: combatData);
			gameNode.AddChild(combatNode);
			WaitForCombatEnd(gameNode: gameNode, combatNode: combatNode);
		}
		async void WaitForCombatEnd(GameNode gameNode, CombatNode combatNode)
		{
			try
			{
				await combatNode;
				if (IsInstanceValid(gameNode) && gameNode.IsInsideTree())
				{
					_ = new IdleState(gameNode);
				}
			}
			catch (Exception e)
			{
				Log.PrintException(e);
				if (IsInstanceValid(gameNode) && gameNode.IsInsideTree())
				{
					gameNode.Root.McpRespond();
				}
			}
		}
	}
	public static GameNode Create(GameData gameData)
	{
		var gameNode = GD.Load<PackedScene>(ResourceTable.game).Instantiate<GameNode>();
		gameNode.gameData = gameData;
		return gameNode;
	}
	State state = null!;
	GameData gameData = null!;
	public ProgramRootNode Root { get; private set; } = null!;
	State CurrentState
	{
		get => state;
		set
		{
			state = value;
			gameData.state = value switch
			{
				IdleState => IdleState.serializeId,
				CombatState => CombatState.serializeId,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}
	public void Save()
	{
		Persistant.Save(gameData, Persistant.saveDataPath);
	}
	public void SetCombatData(CombatData? combatData)
	{
		gameData.combatData = combatData;
	}
	public override void _Ready()
	{
		Root = GetParent<ProgramRootNode>();
		_ = State.Create(gameNode: this);
	}
	public override void _ExitTree()
	{
		state.OnExit?.Invoke();
	}
}
