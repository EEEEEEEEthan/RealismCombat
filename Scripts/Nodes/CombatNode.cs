using System;
using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
	public abstract class State(CombatNode combatNode)
	{
		public static State Create(CombatNode combatNode)
		{
			var combatData = combatNode.combatData;
			if (combatData.characters.Count == 0) throw new InvalidOperationException("战斗数据中没有角色");
			return combatData.state switch
			{
				RoundInProgressState.serializeId => new RoundInProgressState(combatNode),
				CharacterTurnState.serializeId => new CharacterTurnState(combatNode: combatNode,
					character: combatData.characters[combatData.currentCharacterIndex]),
				CharacterTurnActionState.serializeId => new CharacterTurnActionState(combatNode: combatNode, action: combatData.lastAction!),
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
		public readonly CombatNode combatNode = combatNode;
		public abstract void Update(double deltaTime);
	}
	public class RoundInProgressState : State
	{
		public const byte serializeId = 0;
		public RoundInProgressState(CombatNode combatNode) : base(combatNode) => combatNode.CurrentState = this;
		public override void Update(double deltaTime)
		{
			foreach (var character in combatNode.combatData.characters)
			{
				character.actionPoint += character.speed * deltaTime;
				if (character.actionPoint == 0)
				{
					combatNode.state = new CharacterTurnState(combatNode: combatNode, character: character);
					break;
				}
			}
		}
	}
	public class CharacterTurnState : State
	{
		public const byte serializeId = 1;
		public readonly CharacterData character;
		public CharacterTurnState(CombatNode combatNode, CharacterData character) : base(combatNode)
		{
			this.character = character;
			var characterIndex = (byte)combatNode.combatData.characters.IndexOf(character);
			combatNode.combatData.currentCharacterIndex = characterIndex;
			combatNode.CurrentState = this;
			HandleCharacterTurn(combatNode: combatNode, character: character, characterIndex: characterIndex);
		}
		public override void Update(double deltaTime) { }
		void HandleCharacterTurn(CombatNode combatNode, CharacterData character, byte characterIndex)
		{
			if (character.PlayerControlled)
			{
				var programRoot = combatNode.GetNode<ProgramRootNode>("/root/ProgramRoot");
				var dialogue = programRoot.CreateDialogue();
				dialogue.Initialize(new()
				{
					title = $"{character.name}的回合",
					options =
					[
						new()
						{
							option = "攻击",
							description = "对敌人发起攻击",
							onConfirm = () =>
							{
								var targetIndex = combatNode.combatData.characters.FindIndex(c => c.team != character.team);
								if (targetIndex == -1) throw new InvalidOperationException("没有找到可攻击的敌人");
								var attackerIndex = characterIndex;
								var action = new ActionData(
									attackerIndex: attackerIndex,
									attackerBody: BodyPartCode.RightArm,
									defenderIndex: targetIndex,
									defenderBody: BodyPartCode.Head
								);
								combatNode.combatData.lastAction = action;
								combatNode.state = new CharacterTurnActionState(combatNode: combatNode, action: action);
								dialogue.QueueFree();
								programRoot.McpRespond();
							},
							available = true,
						},
					],
				});
			}
			else
			{
				var targetIndex = combatNode.combatData.characters.FindIndex(c => c.team != character.team);
				if (targetIndex == -1) throw new InvalidOperationException("没有找到可攻击的敌人");
				var attackerIndex = characterIndex;
				var action = new ActionData(
					attackerIndex: attackerIndex,
					attackerBody: BodyPartCode.RightArm,
					defenderIndex: targetIndex,
					defenderBody: BodyPartCode.Head
				);
				combatNode.combatData.lastAction = action;
				combatNode.state = new CharacterTurnActionState(combatNode: combatNode, action: action);
			}
		}
	}
	public class CharacterTurnActionState : State
	{
		public const byte serializeId = 2;
		public readonly ActionData action;
		public CharacterTurnActionState(CombatNode combatNode, ActionData action) : base(combatNode)
		{
			this.action = action;
			combatNode.CurrentState = this;
			Run();
		}
		void Run()
		{
			
		}
		public override void Update(double deltaTime) { }
	}
	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		combatNode.CurrentState = State.Create(combatNode: combatNode);
		return combatNode;
	}
	State state = null!;
	CombatData combatData = null!;
	GameNode gameNode = null!;
	[Export] Container characterContainer = null!;
	State CurrentState
	{
		get => state;
		set
		{
			state = value;
			combatData.state = value switch
			{
				RoundInProgressState => RoundInProgressState.serializeId,
				CharacterTurnState => CharacterTurnState.serializeId,
				CharacterTurnActionState => CharacterTurnActionState.serializeId,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}
	public override void _Ready()
	{
		foreach (var character in combatData.characters)
		{
			var characterNode = CharacterNode.Create();
			characterNode.CharacterData = character;
			characterContainer.AddChild(characterNode);
		}
	}
	public override void _Process(double delta) => CurrentState.Update(delta);
}
