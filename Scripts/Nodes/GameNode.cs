using System;
using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
public partial class GameNode : Node
{
	public abstract class State(GameNode gameNode)
	{
		public static State Create(GameNode gameNode) =>
			gameNode.gameData.state switch
			{
				IdleState.serializeId => new IdleState(gameNode),
				CombatState.serializeId => new CombatState(gameNode, gameNode.gameData.combatData ?? throw new InvalidOperationException("战斗数据为空")),
				_ => throw new ArgumentOutOfRangeException(),
			};
		public readonly GameNode gameNode = gameNode;
	}
	public class IdleState : State
	{
		public const int serializeId = 0;
		public IdleState(GameNode gameNode) : base(gameNode)
		{
			gameNode.CurrentState = this;
			var dialogue = gameNode.root.CreateDialogue();
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
							dialogue.QueueFree();
							var combatData = new CombatData();
							combatData.characters.Add(new(name: "ethan", team: 0) { actionPoint = 0, });
							combatData.characters.Add(new(name: "dove", team: 1) { actionPoint = 0, });
							gameNode.CurrentState = new CombatState(gameNode: gameNode, combatData: combatData);
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
							dialogue.QueueFree();
							gameNode.QueueFree();
							gameNode.root.state = new ProgramRootNode.IdleState(gameNode.root);
						},
						available = true,
					},
				],
			});
		}
	}
	public class CombatState : State
	{
		public const int serializeId = 1;
		public CombatState(GameNode gameNode, CombatData combatData) : base(gameNode)
		{
			gameNode.CurrentState = this;
			gameNode.gameData.combatData = combatData;
			var combatNode = CombatNode.Create(gameNode: gameNode, combatData: combatData);
			gameNode.AddChild(combatNode);
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
	ProgramRootNode root = null!;
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
	public override void _Ready()
	{
		root = GetParent<ProgramRootNode>();
		CurrentState = State.Create(gameNode: this);
	}
}
