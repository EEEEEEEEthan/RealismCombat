using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using RealismCombat.Data;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
	public class ActionSimulate(CombatData combatData)
	{
		public readonly CombatData combatData = combatData;
		public int? attackerIndex;
		public int? defenderIndex;
		public BodyPartCode? attackerBodyPart;
		public BodyPartCode? defenderBodyPart;
		CharacterData Attacker
		{
			get
			{
				if (!attackerIndex.HasValue) throw new InvalidOperationException("攻击者索引未设置");
				return combatData.characters[attackerIndex.Value];
			}
		}
		public bool ValidAttackerBodyPart(BodyPartCode bodyPart, out string error)
		{
			if (Attacker.bodyParts[(int)bodyPart].hp <= 0)
			{
				error = $"{Attacker.name}的{bodyPart.GetName()}已失去战斗能力";
				return false;
			}
			error = null!;
			return true;
		}
	}
	public class CombatNodeAwaiter : INotifyCompletion
	{
		readonly CombatNode combatNode;
		Action? continuation;
		public bool IsCompleted => !IsInstanceValid(combatNode) || !combatNode.IsInsideTree();
		public CombatNodeAwaiter(CombatNode combatNode)
		{
			this.combatNode = combatNode;
			combatNode.TreeExiting += OnTreeExiting;
		}
		public void OnCompleted(Action continuation) => this.continuation = continuation;
		public void GetResult() { }
		void OnTreeExiting()
		{
			combatNode.TreeExiting -= OnTreeExiting;
			continuation?.Invoke();
		}
	}
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
		bool firstUpdate = true;
		public RoundInProgressState(CombatNode combatNode) : base(combatNode)
		{
			combatNode.CurrentState = this;
			var allActionPoints = string.Join(separator: ", ", values: combatNode.combatData.characters.Select(c => $"{c.name}:{c.actionPoint:F2}"));
		}
		public override void Update(double deltaTime)
		{
			if (firstUpdate)
			{
				firstUpdate = false;
				CheckForReadyCharacter();
				return;
			}
			foreach (var character in combatNode.combatData.characters)
			{
				var oldActionPoint = character.actionPoint;
				character.actionPoint += character.speed * deltaTime;
				if (oldActionPoint < 0 && character.actionPoint >= 0) Log.Print($"{character.name} 行动力恢复到 {character.actionPoint:F2}");
			}
			CheckForReadyCharacter();
		}
		void CheckForReadyCharacter()
		{
			foreach (var character in combatNode.combatData.characters)
				if (character.actionPoint >= 0)
				{
					_ = new CharacterTurnState(combatNode: combatNode, character: character);
					return;
				}
		}
	}
	public class CharacterTurnState : State
	{
		public const byte serializeId = 1;
		static int GetBodyPartHp(CharacterData character, BodyPartCode bodyPart) =>
			bodyPart switch
			{
				BodyPartCode.Head => character.head.hp,
				BodyPartCode.Chest => character.chest.hp,
				BodyPartCode.LeftArm => character.leftArm.hp,
				BodyPartCode.RightArm => character.rightArm.hp,
				BodyPartCode.LeftLeg => character.leftLeg.hp,
				BodyPartCode.RightLeg => character.rightLeg.hp,
				_ => throw new ArgumentOutOfRangeException(paramName: nameof(bodyPart), actualValue: bodyPart, message: null),
			};
		public readonly CharacterData character;
		public CharacterTurnState(CombatNode combatNode, CharacterData character) : base(combatNode)
		{
			this.character = character;
			var characterIndex = (byte)combatNode.combatData.characters.IndexOf(character);
			combatNode.combatData.currentCharacterIndex = characterIndex;
			combatNode.CurrentState = this;
			HandleCharacterTurn(combatNode: combatNode, attacker: character, attackerIndex: characterIndex);
		}
		public override void Update(double deltaTime) { }
		void HandleCharacterTurn(CombatNode combatNode, CharacterData attacker, byte attackerIndex)
		{
			if (attacker.PlayerControlled)
			{
				var programRoot = combatNode.GetNode<ProgramRootNode>("/root/ProgramRoot");
				var simulate = new ActionSimulate(this.combatNode.combatData)
				{
					attackerIndex = attackerIndex,
				};
				selectAttackerBody();
				void selectAttackerBody()
				{
					var dialogue = programRoot.CreateDialogue();
					var dialogueData = new DialogueData
					{
						title = $"{attacker.name}的回合-选择身体部位",
					};
					var options = new List<DialogueOptionData>();
					dialogueData.options = options;
					foreach (var b in BodyPartData.allBodyParts)
					{
						var bodyPart = b;
						var available = simulate.ValidAttackerBodyPart(bodyPart: bodyPart, error: out var error);
						options.Add(new()
						{
							available = available,
							description = available ? $"使用{bodyPart.GetName()}进行行动" : error,
							onConfirm = () =>
							{
								simulate.attackerBodyPart = bodyPart;
								dialogue.QueueFree();
								selectDefender();
							},
							option = $"{bodyPart.GetName()}",
						});
					}
					dialogue.Initialize(dialogueData);
				}
				void selectDefender()
				{
					var dialogue = programRoot.CreateDialogue();
					var dialogueData = new DialogueData
					{
						title = $"{attacker.name}的回合-选择目标角色",
					};
					var options = new List<DialogueOptionData>();
					dialogueData.options = options;
					for (var i = 0; i < this.combatNode.combatData.characters.Count; i++)
					{
						var index = i;
						var c = this.combatNode.combatData.characters[i];
						var defender = c;
						if (c.team == attacker.team) continue;
						options.Add(new()
						{
							available = true,
							description = $"以{defender.name}为目标",
							onConfirm = () =>
							{
								dialogue.QueueFree();
								simulate.defenderIndex = index;
								selectDefenderBody();
							},
							option = $"{defender.name}",
						});
					}
					dialogue.Initialize(dialogueData);
				}
				void selectDefenderBody()
				{
					var defender = this.combatNode.combatData.characters[simulate.defenderIndex.Value];
					var dialogue = programRoot.CreateDialogue();
					var dialogueData = new DialogueData
					{
						title = $"{attacker.name}的回合-选择目标身体部位",
					};
					var options = new List<DialogueOptionData>();
					dialogueData.options = options;
					foreach (var b in BodyPartData.allBodyParts)
					{
						var bodyPart = b;
						var available = defender.bodyParts[(int)bodyPart].hp > 0;
						options.Add(new()
						{
							available = available,
							description = available ? $"攻击{defender.name}的{bodyPart.GetName()}" : $"{defender.name}的{bodyPart.GetName()}已失去战斗能力",
							onConfirm = () =>
							{
								simulate.defenderBodyPart = bodyPart;
								dialogue.QueueFree();
								var action = new ActionData(
									attackerIndex: attackerIndex,
									attackerBody: simulate.attackerBodyPart.Value,
									defenderIndex: simulate.defenderIndex.Value,
									defenderBody: simulate.defenderBodyPart.Value
								);
								combatNode.combatData.lastAction = action;
								_ = new CharacterTurnActionState(combatNode: combatNode, action: action);
							},
							option = $"{bodyPart.GetName()}",
						});
					}
					dialogue.Initialize(dialogueData);
				}
			}
			else
			{
				var targetIndex = combatNode.combatData.characters.FindIndex(c => c.team != attacker.team);
				if (targetIndex == -1) throw new InvalidOperationException("没有找到可攻击的敌人");
				var action = new ActionData(
					attackerIndex: attackerIndex,
					attackerBody: BodyPartCode.RightArm,
					defenderIndex: targetIndex,
					defenderBody: BodyPartCode.Head
				);
				combatNode.combatData.lastAction = action;
				_ = new CharacterTurnActionState(combatNode: combatNode, action: action);
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
		public override void Update(double deltaTime) { }
		async void Run()
		{
			try
			{
				var attacker = combatNode.combatData.characters[action.attackerIndex];
				var defender = combatNode.combatData.characters[action.defenderIndex];
				var targetBodyPart = action.defenderBody switch
				{
					BodyPartCode.Head => defender.head,
					BodyPartCode.Chest => defender.chest,
					BodyPartCode.LeftArm => defender.leftArm,
					BodyPartCode.RightArm => defender.rightArm,
					BodyPartCode.LeftLeg => defender.leftLeg,
					BodyPartCode.RightLeg => defender.rightLeg,
					_ => throw new ArgumentOutOfRangeException(),
				};
				await combatNode.gameNode.Root.PopMessage(
					$"{attacker.name}用{action.attackerBody.GetName()}攻击{defender.name}的{action.defenderBody.GetName()}!");
				const int damage = 2;
				targetBodyPart.hp -= damage;
				await combatNode.gameNode.Root.PopMessage($"造成{damage}点伤害!");
				if (defender.Dead)
				{
					await combatNode.gameNode.Root.PopMessage($"{defender.name} 死亡");
					combatNode.combatData.characters.RemoveAt(action.defenderIndex);
					var team0Alive = combatNode.combatData.characters.Exists(c => c.team == 0);
					var team1Alive = combatNode.combatData.characters.Exists(c => c.team == 1);
					if (!team0Alive || !team1Alive)
					{
						var winner = team0Alive ? "玩家" : "敌人";
						Log.Print($"战斗结束，{winner}获胜");
						combatNode.QueueFree();
						return;
					}
				}
				const int actionPoint = 5;
				attacker.actionPoint -= actionPoint;
				await combatNode.gameNode.Root.PopMessage($"{attacker.name}消耗了{actionPoint}行动力!");
				_ = new RoundInProgressState(combatNode);
			}
			catch (Exception e)
			{
				Log.PrintException(e);
				combatNode.gameNode.Root.McpRespond();
			}
		}
	}
	public static CombatNode Create(GameNode gameNode, CombatData combatData)
	{
		var combatNode = GD.Load<PackedScene>(ResourceTable.combat).Instantiate<CombatNode>();
		combatNode.gameNode = gameNode;
		combatNode.combatData = combatData;
		_ = State.Create(combatNode: combatNode);
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
	public CombatNodeAwaiter GetAwaiter() => new(this);
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
