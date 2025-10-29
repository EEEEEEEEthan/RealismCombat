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
				0 => new IdleState(gameNode),
				1 => new CombatState(gameNode),
				_ => throw new ArgumentOutOfRangeException(),
			};
		public readonly GameNode gameNode = gameNode;
	}
	public class IdleState : State
	{
		public IdleState(GameNode gameNode) : base(gameNode)
		{
			gameNode.state = this;
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
							gameNode.state = new CombatState(gameNode: gameNode);
							dialogue.QueueFree();
							var combatData = new CombatData();
							var combatNode = CombatNode.Create(gameNode: this.gameNode, combatData: combatData);
							gameNode.AddChild(combatNode);
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
		public CombatState(GameNode gameNode) : base(gameNode) => gameNode.state = this;
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
	public override void _Ready()
	{
		root = GetParent<ProgramRootNode>();
		state = State.Create(gameNode: this);
	}
}
