using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using RealismCombat.Data;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
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
		static int GetBodyPartHp(CharacterData character, BodyPartCode bodyPart) => bodyPart switch
		{
			BodyPartCode.Head => character.head.hp,
			BodyPartCode.Chest => character.chest.hp,
			BodyPartCode.LeftArm => character.leftArm.hp,
			BodyPartCode.RightArm => character.rightArm.hp,
			BodyPartCode.LeftLeg => character.leftLeg.hp,
			BodyPartCode.RightLeg => character.rightLeg.hp,
			_ => throw new ArgumentOutOfRangeException(nameof(bodyPart), bodyPart, null)
		};
		void HandleCharacterTurn(CombatNode combatNode, CharacterData character, byte characterIndex)
		{
			if (character.PlayerControlled)
			{
				var programRoot = combatNode.GetNode<ProgramRootNode>("/root/ProgramRoot");
				var dialogue = programRoot.CreateDialogue();

				// Step 1: Select your own body part
				var bodyParts = new[]
				{
					new { Code = BodyPartCode.Head, Name = "头部" },
					new { Code = BodyPartCode.Chest, Name = "胸部" },
					new { Code = BodyPartCode.LeftArm, Name = "左臂" },
					new { Code = BodyPartCode.RightArm, Name = "右臂" },
					new { Code = BodyPartCode.LeftLeg, Name = "左腿" },
					new { Code = BodyPartCode.RightLeg, Name = "右腿" },
				};

				dialogue.Initialize(new()
				{
					title = $"{character.name}的回合 - 选择使用的身体部位",
					options = bodyParts.Select(part => new DialogueOptionData
					{
						option = part.Name,
						description = $"使用{part.Name}进行行动",
						onConfirm = () =>
						{
							var attackerBody = part.Code;

							// Step 2: Select "Attack" action
							var actionDialogue = programRoot.CreateDialogue();
							var actionOptions = new[]
							{
								new DialogueOptionData
								{
									option = "攻击",
									description = "对敌人发起攻击",
									onConfirm = () =>
									{
										var enemies = combatNode.combatData.characters
											.Select((c, i) => new { Character = c, Index = i })
											.Where(x => x.Character.team != character.team)
											.ToList();
										if (enemies.Count == 0) throw new InvalidOperationException("没有找到可攻击的敌人");

										// Step 3: Select enemy
										var enemyDialogue = programRoot.CreateDialogue();
										var enemyOptions = enemies.Select(enemy => new DialogueOptionData
										{
											option = enemy.Character.name,
											description = $"选择攻击 {enemy.Character.name}",
											onConfirm = () =>
											{
												// Step 4: Select enemy's body part
												var bodyPartDialogue = programRoot.CreateDialogue();
												var bodyPartOptions = bodyParts.Select(targetPart => new DialogueOptionData
												{
													option = targetPart.Name,
													description = $"攻击{enemy.Character.name}的{targetPart.Name} (生命值: {GetBodyPartHp(enemy.Character, targetPart.Code)})",
													onConfirm = () =>
													{
														var attackerIndex = characterIndex;
														var action = new ActionData(
															attackerIndex: attackerIndex,
															attackerBody: attackerBody,
															defenderIndex: (byte)enemy.Index,
															defenderBody: targetPart.Code
														);
														combatNode.combatData.lastAction = action;
														_ = new CharacterTurnActionState(combatNode: combatNode, action: action);
														bodyPartDialogue.QueueFree();
														enemyDialogue.QueueFree();
														actionDialogue.QueueFree();
														dialogue.QueueFree();
													},
													available = true,
												}).ToList();

												// Add back button for body part selection
												bodyPartOptions.Add(new DialogueOptionData
												{
													option = "返回",
													description = "返回选择敌人",
													onConfirm = () =>
													{
														bodyPartDialogue.QueueFree();
													},
													available = true,
												});

												bodyPartDialogue.Initialize(new()
												{
													title = $"选择攻击 {enemy.Character.name} 的部位",
													options = bodyPartOptions.ToArray(),
												});
											},
											available = true,
										}).ToList();

										// Add back button for enemy selection
										enemyOptions.Add(new DialogueOptionData
										{
											option = "返回",
											description = "返回选择行动",
											onConfirm = () =>
											{
												enemyDialogue.QueueFree();
											},
											available = true,
										});

										enemyDialogue.Initialize(new()
										{
											title = "选择攻击目标",
											options = enemyOptions.ToArray(),
										});
									},
									available = true,
								},
							};

							// Add back button for action selection
							var actionOptionsList = actionOptions.ToList();
							actionOptionsList.Add(new DialogueOptionData
							{
								option = "返回",
								description = "返回选择身体部位",
								onConfirm = () =>
								{
									actionDialogue.QueueFree();
								},
								available = true,
							});

							actionDialogue.Initialize(new()
							{
								title = "选择行动",
								options = actionOptionsList.ToArray(),
							});
						},
						available = true,
					}).ToArray(),
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
