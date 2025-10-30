using System;
using System.Collections.Generic;
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
							dialogue?.QueueFree();
							ShowInventoryMenu(gameNode: gameNode);
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
					options.Add(new()
					{
						option = $"物品{item.itemId} x{item.count}",
						description = null,
						onPreview = () => { },
						onConfirm = () => { },
						available = false,
					});
				}
			}
			options.Add(new()
			{
				option = "返回",
				description = "返回游戏菜单",
				onPreview = () => { },
				onConfirm = () =>
				{
					inventoryDialogue?.QueueFree();
					_ = new IdleState(gameNode);
				},
				available = true,
			});
			inventoryDialogue.Initialize(new()
			{
				title = "物品栏",
				options = options,
			});
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
