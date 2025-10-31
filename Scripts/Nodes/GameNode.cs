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
							CombatData combatData;
							if (gameNode.gameData.combatData != null && gameNode.gameData.combatData.characters.Count > 0)
							{
								combatData = gameNode.gameData.combatData;
								var (min, max) = CharacterData.InitialActionPointRange;
								foreach (var character in combatData.characters)
								{
									character.ActionPoint = GD.Randf() * (max - min) + min;
								}
							}
							else
							{
								combatData = new CombatData();
								var (min, max) = CharacterData.InitialActionPointRange;
								combatData.characters.Add(new(name: "ethan", team: 0) { ActionPoint = GD.Randf() * (max - min) + min, });
								combatData.characters.Add(new(name: "dove", team: 1) { ActionPoint = GD.Randf() * (max - min) + min, });
							}
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
			void RefreshMenu()
			{
				if (!IsInstanceValid(equipmentDialogue) || !equipmentDialogue.IsInsideTree()) return;
				equipmentDialogue.QueueFree();
				ShowEquipmentMenu(gameNode: gameNode);
			}
			foreach (var bodyPart in character.bodyParts)
			{
				var container = (IItemContainer)bodyPart;
				container.ItemsChanged += RefreshMenu;
			}
			equipmentDialogue.TreeExited += () =>
			{
				foreach (var bodyPart in character.bodyParts)
				{
					var container = (IItemContainer)bodyPart;
					container.ItemsChanged -= RefreshMenu;
				}
			};
			var options = new List<DialogueOptionData>();
			foreach (var bodyPart in character.bodyParts)
			{
				var partName = bodyPart.id.GetName();
				var container = (IItemContainer)bodyPart;
				var equippedItems = new List<string>();
				foreach (var slot in container.items)
				{
					if (slot != null)
					{
						equippedItems.Add(GetItemName(slot.itemId));
					}
				}
				var optionText = equippedItems.Count == 0 ? partName : $"{partName}[{string.Join("][", equippedItems)}]";
				options.Add(new()
				{
					option = optionText,
					description = null,
					onPreview = () => { },
					onConfirm = () =>
					{
						ShowItemContainerMenu(gameNode: gameNode, container: container, title: partName, parentDialogue: equipmentDialogue, onReturn: () => { }, returnDescription: "返回装备菜单");
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
		static void ShowItemContainerMenu(GameNode gameNode, IItemContainer container, string title, MenuDialogue? parentDialogue, Action? onReturn, string? returnDescription)
		{
			var containerDialogue = gameNode.Root.CreateDialogue();
			void RefreshMenu()
			{
				if (!IsInstanceValid(containerDialogue) || !containerDialogue.IsInsideTree()) return;
				containerDialogue.QueueFree();
				ShowItemContainerMenu(gameNode: gameNode, container: container, title: title, parentDialogue: parentDialogue, onReturn: onReturn, returnDescription: returnDescription);
			}
			container.ItemsChanged += RefreshMenu;
			containerDialogue.TreeExited += () =>
			{
				container.ItemsChanged -= RefreshMenu;
			};
			var options = new List<DialogueOptionData>();
			var isItemData = container is ItemData;
			for (var i = 0; i < container.items.Count; i++)
			{
				var slotIndex = i;
				var slotItem = container.items[i];
				string optionText;
				if (slotItem == null)
				{
					optionText = "空";
				}
				else if (isItemData && slotItem.items.Count > 0)
				{
					var equippedCount = slotItem.items.Count(slot => slot != null);
					var totalSlots = slotItem.items.Count;
					optionText = $"{GetItemName(slotItem.itemId)}({equippedCount}/{totalSlots})";
				}
				else
				{
					optionText = GetItemName(slotItem.itemId);
				}
				options.Add(new()
				{
					option = optionText,
					description = null,
					onPreview = () => { },
					onConfirm = () =>
					{
						if (slotItem == null)
						{
							ShowItemSelectMenu(gameNode: gameNode, container: container, slotIndex: slotIndex, title: title, parentDialogue: parentDialogue, containerDialogue: containerDialogue, onReturn: onReturn, returnDescription: returnDescription);
						}
						else
						{
							if (slotItem.items.Count > 0)
							{
								ShowItemContainerMenu(gameNode: gameNode, container: slotItem, title: GetItemName(slotItem.itemId), parentDialogue: containerDialogue, onReturn: onReturn, returnDescription: "返回上级菜单");
							}
							else
							{
								ShowItemUnEquipMenu(gameNode: gameNode, container: container, slotIndex: slotIndex, slotItem: slotItem, title: title, parentDialogue: parentDialogue, containerDialogue: containerDialogue, onReturn: onReturn, returnDescription: returnDescription);
							}
						}
					},
					available = true,
				});
			}
			containerDialogue.Initialize(new()
			{
				title = title,
				options = options,
			}, onReturn: onReturn, returnDescription: returnDescription);
		}
		static void ShowItemSelectMenu(GameNode gameNode, IItemContainer container, int slotIndex, string title, MenuDialogue? parentDialogue, MenuDialogue containerDialogue, Action? onReturn, string? returnDescription)
		{
			var selectDialogue = gameNode.Root.CreateDialogue();
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
							var newItem = new ItemData(itemId: itemId, count: 1);
							if (container is BodyPartData bodyPart)
							{
								bodyPart.SetSlot(slotIndex, newItem);
							}
							else if (container is ItemData itemContainer)
							{
								itemContainer.SetSlot(slotIndex, newItem);
							}
							if (item.count > 1)
							{
								item.count--;
							}
							else
							{
								gameNode.gameData.items.Remove(item);
							}
							gameNode.Save();
							selectDialogue?.QueueFree();
							containerDialogue?.QueueFree();
							if (parentDialogue != null)
							{
								parentDialogue?.QueueFree();
							}
						},
						available = true,
					});
				}
			}
			selectDialogue.Initialize(new()
			{
				title = $"{title} - 选择物品",
				options = options,
			}, onReturn: () => { }, returnDescription: "返回上级菜单");
		}
		static void ShowItemUnEquipMenu(GameNode gameNode, IItemContainer container, int slotIndex, ItemData slotItem, string title, MenuDialogue? parentDialogue, MenuDialogue containerDialogue, Action? onReturn, string? returnDescription)
		{
			var unEquipDialogue = gameNode.Root.CreateDialogue();
			var options = new List<DialogueOptionData>();
			options.Add(new()
			{
				option = "卸下",
				description = "卸下这个装备",
				onPreview = () => { },
				onConfirm = () =>
				{
					AddItemToInventory(gameNode: gameNode, item: slotItem);
					if (container is BodyPartData bodyPart)
					{
						bodyPart.SetSlot(slotIndex, null);
					}
					else if (container is ItemData itemContainer)
					{
						itemContainer.SetSlot(slotIndex, null);
					}
					gameNode.Save();
					unEquipDialogue?.QueueFree();
					containerDialogue?.QueueFree();
					if (parentDialogue != null)
					{
						parentDialogue?.QueueFree();
					}
				},
				available = true,
			});
			unEquipDialogue.Initialize(new()
			{
				title = $"{title} - {GetItemName(slotItem.itemId)}",
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
