using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Godot;
using RealismCombat.Data;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Nodes;
public partial class CombatNode : Node
{
	public class ActionSimulate(CombatData combatData)
	{
		public class SimulateResult
		{
			public (int min, int max) damageRange;
			public int actionPoint;
		}
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
		CharacterData Defender
		{
			get
			{
				if (!defenderIndex.HasValue) throw new InvalidOperationException("防御者索引未设置");
				return combatData.characters[defenderIndex.Value];
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
		public bool ValidDefenderBodyPart(BodyPartCode bodyPart, out string error)
		{
			if (Defender.bodyParts[(int)bodyPart].hp <= 0)
			{
				error = $"{Defender.name}的{bodyPart.GetName()}已失去战斗能力";
				return false;
			}
			error = null!;
			return true;
		}
		public bool ValidDefender(int defenderIndex, out string error)
		{
			var defender = combatData.characters[defenderIndex];
			if (defenderIndex == attackerIndex)
			{
				error = "不能攻击自己";
				return false;
			}
			if (defender.team == Attacker.team)
			{
				error = "不能攻击同队队友";
				return false;
			}
			if (defender.Dead)
			{
				error = $"{defender.name}已死亡";
				return false;
			}
			error = null!;
			return true;
		}
		public SimulateResult Simulate()
		{
			if (!attackerIndex.HasValue) throw new InvalidOperationException("攻击者索引未设置");
			if (!defenderIndex.HasValue) throw new InvalidOperationException("防御者索引未设置");
			if (!attackerBodyPart.HasValue) throw new InvalidOperationException("攻击者身体部位未设置");
			if (!defenderBodyPart.HasValue) throw new InvalidOperationException("防御者身体部位未设置");
			if (!ValidAttackerBodyPart(bodyPart: attackerBodyPart.Value, error: out var attackerError)) throw new InvalidOperationException(attackerError);
			if (!ValidDefender(defenderIndex: defenderIndex.Value, error: out var defenderError)) throw new InvalidOperationException(defenderError);
			if (!ValidDefenderBodyPart(bodyPart: defenderBodyPart.Value, error: out var defenderBodyError))
				throw new InvalidOperationException(defenderBodyError);
			return new()
			{
				damageRange = (1, 3),
				actionPoint = 5,
			};
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
			combatNode.gameNode.Save();
		}
		public override void Update(double deltaTime)
		{
			if (!combatNode.AreAllEntryAnimationsFinished()) return;
			if (firstUpdate)
			{
				firstUpdate = false;
				CheckForReadyCharacter();
				return;
			}
			var speedMultiplier = Input.IsAnythingPressed() ? 5.0 : 1.0;
			foreach (var character in combatNode.combatData.characters)
			{
				var oldActionPoint = character.ActionPoint;
				character.ActionPoint += character.speed * speedMultiplier * deltaTime;
				if (oldActionPoint < 0 && character.ActionPoint >= 0) Log.Print($"{character.name} 行动力恢复到 {character.ActionPoint:F2}");
			}
			CheckForReadyCharacter();
		}
		void CheckForReadyCharacter()
		{
			foreach (var character in combatNode.combatData.characters)
				if (character.ActionPoint >= 0)
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
			combatNode.gameNode.Save();
			combatNode.gameNode.Root.PlaySoundEffect(AudioTable.selection3);
			var attackerNode = combatNode.GetCharacterNode(character);
			if (attackerNode != null) attackerNode.IsActing = true;
			if (character.PlayerControlled)
				HandlePlayerTurn(combatNode: combatNode, attacker: character, attackerIndex: characterIndex);
			else
				HandleBotTurn(combatNode: combatNode, attacker: character, attackerIndex: characterIndex);
		}
		public override void Update(double deltaTime) { }
		void HandlePlayerTurn(CombatNode combatNode, CharacterData attacker, byte attackerIndex)
		{
			var programRoot = combatNode.gameNode.Root;
			var simulate = new ActionSimulate(this.combatNode.combatData)
			{
				attackerIndex = attackerIndex,
			};
			MenuDialogue? attackerBodyDialogue = null;
			MenuDialogue? defenderDialogue = null;
			MenuDialogue? defenderBodyDialogue = null;
			selectAttackerBody();
			string GetBodyPartEquipmentText(CharacterData character, BodyPartCode bodyPart)
			{
				var bodyPartData = bodyPart switch
				{
					BodyPartCode.Head => character.head,
					BodyPartCode.Chest => character.chest,
					BodyPartCode.LeftArm => character.leftArm,
					BodyPartCode.RightArm => character.rightArm,
					BodyPartCode.LeftLeg => character.leftLeg,
					BodyPartCode.RightLeg => character.rightLeg,
					_ => throw new ArgumentOutOfRangeException(),
				};
				var equippedItems = new List<string>();
				foreach (var slot in bodyPartData.slots)
				{
					if (slot.item != null)
					{
						var itemName = ItemConfig.Configs.TryGetValue(slot.item.itemId, out var config) ? config.name : $"物品{slot.item.itemId}";
						equippedItems.Add(itemName);
					}
				}
				if (equippedItems.Count == 0)
				{
					return string.Empty;
				}
				return $"[{string.Join("][", equippedItems)}]";
			}
			void selectAttackerBody()
			{
				attackerBodyDialogue = programRoot.CreateDialogue();
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
					var equipmentText = GetBodyPartEquipmentText(attacker, bodyPart);
					options.Add(new()
					{
						available = available,
						description = available ? $"使用{bodyPart.GetName()}进行行动" : error,
						onConfirm = () =>
						{
							simulate.attackerBodyPart = bodyPart;
							selectDefender();
						},
						option = $"{bodyPart.GetName()}{equipmentText}",
					});
				}
				attackerBodyDialogue.Initialize(dialogueData);
			}
			void selectDefender()
			{
				defenderDialogue = programRoot.CreateDialogue();
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
					var available = simulate.ValidDefender(defenderIndex: index, error: out var error);
					options.Add(new()
					{
						available = available,
						description = available ? $"以{defender.name}为目标" : error,
						onConfirm = () =>
						{
							simulate.defenderIndex = index;
							selectDefenderBody();
						},
						option = $"{defender.name}",
					});
				}
				defenderDialogue.Initialize(data: dialogueData, onReturn: () => { simulate.defenderIndex = null; }, returnDescription: "返回选择身体部位");
			}
			void selectDefenderBody()
			{
				var defender = this.combatNode.combatData.characters[simulate.defenderIndex.Value];
				defenderBodyDialogue = programRoot.CreateDialogue();
				var dialogueData = new DialogueData
				{
					title = $"{attacker.name}的回合-选择目标身体部位",
				};
				var options = new List<DialogueOptionData>();
				dialogueData.options = options;
				foreach (var b in BodyPartData.allBodyParts)
				{
					var bodyPart = b;
					var available = simulate.ValidDefenderBodyPart(bodyPart: bodyPart, error: out var error);
					var equipmentText = GetBodyPartEquipmentText(defender, bodyPart);
					options.Add(new()
					{
						available = available,
						description = available ? $"攻击{defender.name}的{bodyPart.GetName()}" : error,
						onConfirm = () =>
						{
							simulate.defenderBodyPart = bodyPart;
							attackerBodyDialogue?.QueueFree();
							defenderDialogue?.QueueFree();
							defenderBodyDialogue?.QueueFree();
							var action = new ActionData(
								attackerIndex: attackerIndex,
								attackerBody: simulate.attackerBodyPart.Value,
								defenderIndex: simulate.defenderIndex.Value,
								defenderBody: simulate.defenderBodyPart.Value
							);
							combatNode.combatData.lastAction = action;
							_ = new CharacterTurnActionState(combatNode: combatNode, action: action);
						},
						option = $"{bodyPart.GetName()}{equipmentText}",
					});
				}
				defenderBodyDialogue.Initialize(data: dialogueData, onReturn: () => { simulate.defenderBodyPart = null; }, returnDescription: "返回选择目标角色");
			}
		}
		void HandleBotTurn(CombatNode combatNode, CharacterData attacker, byte attackerIndex)
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
				var simulate = new ActionSimulate(combatNode.combatData)
				{
					attackerIndex = action.attackerIndex,
					defenderIndex = action.defenderIndex,
					attackerBodyPart = action.attackerBody,
					defenderBodyPart = action.defenderBody,
				};
				var simulateResult = simulate.Simulate();
				var attacker = combatNode.combatData.characters[action.attackerIndex];
				var defender = combatNode.combatData.characters[action.defenderIndex];
				var defenderNode = combatNode.GetCharacterNode(defender);
				if (defenderNode != null) defenderNode.IsActing = true;
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
				var damage = GD.RandRange(from: simulateResult.damageRange.min, to: simulateResult.damageRange.max);
				targetBodyPart.hp -= damage;
				combatNode.gameNode.Root.PlaySoundEffect(AudioTable.retrohurt1236672);
				if (defenderNode != null)
				{
					defenderNode.Shake();
					var bodyPartDrawer = defenderNode.GetBodyPartDrawer(action.defenderBody);
					bodyPartDrawer?.Flash();
				}
				await combatNode.gameNode.Root.PopMessage($"造成{damage}点伤害!");
				if (defenderNode != null) defenderNode.IsActing = false;
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
						combatNode.combatData.characters.Clear();
						var gameNode = combatNode.gameNode;
						if (!team0Alive)
						{
							var root = gameNode.Root;
							root.PlayMusic(AudioTable.arpegio01Loop45094);
							await root.PopMessage("玩家队伍全灭，返回主菜单");
							if (File.Exists(Persistant.saveDataPath))
							{
								File.Delete(Persistant.saveDataPath);
								Log.Print("存档已删除");
							}
							combatNode.QueueFree();
							gameNode.QueueFree();
							root.state = new ProgramRootNode.IdleState(root);
						}
						else
						{
							gameNode.SetCombatData(null);
							gameNode.Save();
							combatNode.QueueFree();
						}
						return;
					}
				}
				var actionPoint = simulateResult.actionPoint;
				attacker.ActionPoint -= actionPoint;
				await combatNode.gameNode.Root.PopMessage($"{attacker.name}消耗了{actionPoint}行动力!");
				var attackerNode = combatNode.GetCharacterNode(attacker);
				if (attackerNode != null) attackerNode.IsActing = false;
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
	readonly Dictionary<CharacterData, CharacterNode> characterNodes = new();
	State state = null!;
	CombatData combatData = null!;
	GameNode gameNode = null!;
	[Export] Container team0 = null!;
	[Export] Container team1 = null!;
	bool isEntryAnimationFinished;
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
			var targetTeam = character.team == 0 ? team0 : team1;
			targetTeam.AddChild(characterNode);
			characterNode.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
			characterNode.SizeFlagsVertical = character.team == 0 ? Control.SizeFlags.ShrinkEnd : Control.SizeFlags.ShrinkBegin;
			characterNodes[character] = characterNode;
		}
		PlayEntryAnimations();
	}
	public override void _Process(double delta) => CurrentState.Update(delta);
	public bool AreAllEntryAnimationsFinished() => isEntryAnimationFinished;
	async void PlayEntryAnimations()
	{
		var screenSize = GetViewport().GetVisibleRect().Size;
		team0.OffsetTop = (int)screenSize.Y;
		team1.OffsetTop = -(int)screenSize.Y;
		var tween0 = CreateTween();
		var tween1 = CreateTween();
		tween0.TweenProperty(@object: team0, property: "offset_top", finalVal: 0, duration: 0.5);
		tween1.TweenProperty(@object: team1, property: "offset_top", finalVal: 0, duration: 0.5);
		await ToSignal(source: tween0, signal: Tween.SignalName.Finished);
		isEntryAnimationFinished = true;
	}
	CharacterNode? GetCharacterNode(CharacterData character) => characterNodes.TryGetValue(key: character, value: out var node) ? node : null;
}
