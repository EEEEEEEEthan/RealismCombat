using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
public class Combat
{
	static void ClearCharacterBuffs(Character character)
	{
		foreach (var bodyPart in character.bodyParts)
		{
			if (bodyPart is IBuffOwner bodyPartBuffOwner) ClearBuffs(bodyPartBuffOwner);
			foreach (var slot in bodyPart.Slots)
				if (slot.Item != null)
					ClearItemBuffs(slot.Item);
		}
		foreach (var item in character.inventory.Items) ClearItemBuffs(item);
	}
	static void ClearItemBuffs(Item item)
	{
		if (item is IBuffOwner buffOwner) ClearBuffs(buffOwner);
		foreach (var slot in item.Slots)
			if (slot.Item != null)
				ClearItemBuffs(slot.Item);
	}
	static void ClearBuffs(IBuffOwner buffOwner) => buffOwner.Buffs.Clear();
	static bool IsContainerOnCharacter(Character character, IItemContainer target)
	{
		foreach (var bodyPart in character.bodyParts)
		{
			if (ReferenceEquals(bodyPart, target)) return true;
			if (IsContainerOnDescendants(bodyPart, target)) return true;
		}
		return false;
	}
	static bool IsContainerOnDescendants(IItemContainer container, IItemContainer target)
	{
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			if (ReferenceEquals(item, target)) return true;
			if (IsContainerOnDescendants(item, target)) return true;
		}
		return false;
	}
	static bool TryRemoveItem(Character character, Item item)
	{
		foreach (var bodyPart in character.bodyParts)
			if (TryRemoveItem(bodyPart, item))
				return true;
		var inventory = character.inventory.Items;
		for (var i = 0; i < inventory.Count; i++)
		{
			if (!ReferenceEquals(inventory[i], item)) continue;
			inventory.RemoveAt(i);
			return true;
		}
		return false;
	}
	static bool TryRemoveItem(IItemContainer container, Item item)
	{
		foreach (var slot in container.Slots)
		{
			if (ReferenceEquals(slot.Item, item))
			{
				slot.Item = null;
				return true;
			}
			if (slot.Item != null && TryRemoveItem(slot.Item, item)) return true;
		}
		return false;
	}
	public readonly CombatNode combatNode;
	public readonly HashSet<Item> droppedItems = [];
	readonly Dictionary<Item, (Character owner, ItemSlot slot)> originalSlots = new();
	readonly PlayerInput playerInput;
	readonly GenericAIInput genericAiInput;
	readonly TaskCompletionSource<bool> taskCompletionSource = new();
	bool hasEnded;
	public double Time { get; private set; }
	public Character? Considering { get; private set; }
	internal Character[] Allies { get; }
	internal Character[] Enemies { get; }
	IEnumerable<Character> AllFighters => Allies.Union(Enemies);
	public Combat(Character[] allies, Character[] enemies, CombatNode combatNode)
	{
		Allies = allies;
		Enemies = enemies;
		foreach (var character in AllFighters) character.reaction = 0;
		foreach (var character in AllFighters) Log.Print($"{character.name} 行动点 {character.actionPoint.value}/{character.actionPoint.maxValue}");
		this.combatNode = combatNode;
		combatNode.Initialize(this);
		CaptureOriginalSlots(Allies);
		CaptureOriginalSlots(Enemies);
		playerInput = new(this);
		genericAiInput = new(this);
		StartLoop();
	}
	public TaskAwaiter<bool> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	internal async Task<ReactionDecision> HandleIncomingAttack(AttackBase attack)
	{
		var actor = attack.actor;
		var target = attack.target!;
		var targetObject = attack.targetObject!;
		CombatInput input = Allies.Contains(target) ? playerInput : genericAiInput;
		var reactionAvailable = target.reaction > 0;
		if (!reactionAvailable && input is GenericAIInput) return ReactionDecision.CreateEndure();
		var decision = await input.MakeReactionDecisionTask(target, actor, targetObject);
		switch (decision.type)
		{
			case ReactionTypeCode.Block when reactionAvailable && decision.blockTarget is { Available: true, }:
				target.reaction = Math.Max(0, target.reaction - 1);
				// 格挡仅使用当前动作的部位或其装备时会打断自身行动
				if (target.combatAction?.WillBeInterruptedByBlockingWith(decision.blockTarget) == true)
				{
					target.combatAction = null;
                    await DialogueManager.ShowGenericDialogue($"{target.name}的行动被打断!");
				}
				// 反应后移除所有招架buff
				foreach (var bodyPart in target.bodyParts)
				{
					bodyPart.Buffs.Remove(BuffCode.Parrying);
				}
				return decision;
			case ReactionTypeCode.Dodge when reactionAvailable:
				target.reaction = Math.Max(0, target.reaction - 1);
				target.combatAction = null;
                await DialogueManager.ShowGenericDialogue($"{target.name}的行动被打断!");
				// 反应后移除所有招架buff
				foreach (var bodyPart in target.bodyParts)
				{
					bodyPart.Buffs.Remove(BuffCode.Parrying);
				}
				return decision;
			case ReactionTypeCode.None:
				return ReactionDecision.CreateEndure();
			default:
				throw new InvalidOperationException("无效的反应选择");
		}
	}
	async void StartLoop()
	{
		var nextBleedingTime = 5.0;
		try
		{
			await DialogueManager.ShowGenericDialogue("战斗开始了!");
			while (true)
			{
				if (CheckBattleOutcome()) return;
				foreach (var character in AllFighters.Where(c => c.IsAlive))
				{
					var action = character.combatAction;
					if (action is not null)
						if (!await action.UpdateTask())
						{
							character.combatAction = null;
							if (CheckBattleOutcome()) return;
						}
				}
				while (TryGetActor(out var actor))
				{
					actor.reaction = 1;
					// 移除该角色所有身体部位的招架buff
					foreach (var bodyPart in actor.bodyParts)
					{
						bodyPart.Buffs.Remove(BuffCode.Parrying);
					}
					// 检查所有对手，如果有招架buff则补满其行动力
					var opponents = Allies.Contains(actor) ? Enemies : Allies;
					foreach (var opponent in opponents.Where(c => c.IsAlive))
					{
						var hasParryingBuff = opponent.bodyParts.Any(part => part.Buffs.ContainsKey(BuffCode.Parrying));
						if (hasParryingBuff)
						{
							opponent.actionPoint.value = opponent.actionPoint.maxValue;
						}
					}
					var actorNode = combatNode.GetCharacterNode(actor);
					using var _ = actorNode.MoveScope(combatNode.GetReadyPosition(actor));
					using var __ = actorNode.ExpandScope();
					Considering = actor;
					await DialogueManager.ShowGenericDialogue($"{actor.name}的回合!");
					CombatInput input = Allies.Contains(actor) ? playerInput : genericAiInput;
					var action = await input.MakeDecisionTask(actor);
					Considering = null;
					actor.combatAction = action;
					await action.StartTask();
					if (CheckBattleOutcome()) return;
				}
				if (CheckBattleOutcome()) return;
				await Task.Delay(100);
				Time += 0.1;
				Log.Print($"{nameof(Time)}={Time:F1}");
				foreach (var character in AllFighters.Where(c => c.IsAlive)) character.actionPoint.value += character.Speed * 0.1;
				while (Time >= nextBleedingTime)
				{
					await ApplyBleedingTick();
					nextBleedingTime += 5.0;
				}
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			EndBattle(false);
		}
	}
	bool TryGetActor(out Character actor)
	{
		var result = AllFighters
			.Where(c => c.IsAlive && c.combatAction == null)
			.FirstOrDefault(c => c.actionPoint.value >= c.actionPoint.maxValue);
		actor = result!;
		return result != null;
	}
	bool CheckBattleOutcome()
	{
		if (!Allies.Any(c => c.IsAlive))
		{
			Log.Print("战斗失败");
			EndBattle(false);
			return true;
		}
		if (!Enemies.Any(c => c.IsAlive))
		{
			Log.Print("敌人被消灭，你胜利了");
			EndBattle(true);
			return true;
		}
		return false;
	}
	void EndBattle(bool victory)
	{
		if (hasEnded) return;
		hasEnded = true;
		RestoreOriginalSlots();
		ClearAllBuffs();
		taskCompletionSource.TrySetResult(victory);
	}
	void ClearAllBuffs()
	{
		foreach (var character in AllFighters) ClearCharacterBuffs(character);
	}
	void CaptureOriginalSlots(IEnumerable<Character> characters)
	{
		foreach (var character in characters) CaptureOriginalSlots(character);
	}
	void CaptureOriginalSlots(Character character)
	{
		foreach (var bodyPart in character.bodyParts) CaptureOriginalSlots(character, bodyPart);
	}
	void CaptureOriginalSlots(Character owner, IItemContainer container)
	{
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			originalSlots[item] = (owner, slot);
			CaptureOriginalSlots(owner, item);
		}
	}
	void RestoreOriginalSlots()
	{
		foreach (var pair in originalSlots)
		{
			var item = pair.Key;
			var owner = pair.Value.owner;
			var slot = pair.Value.slot;
			if (!IsContainerOnCharacter(owner, slot.Container)) continue;
			var removed = TryRemoveItem(owner, item);
			var stillOwned = removed || droppedItems.Contains(item);
			if (!stillOwned) continue;
			var occupied = slot.Item;
			if (occupied != null && !ReferenceEquals(occupied, item))
			{
				slot.Item = null;
				owner.inventory.Items.Add(occupied);
			}
			slot.Item = item;
			droppedItems.Remove(item);
		}
	}
	async Task ApplyBleedingTick()
	{
		var events = new List<(Character character, BodyPart part)>();
		foreach (var character in AllFighters.Where(c => c.IsAlive))
		{
			var bleedingParts = character.bodyParts.Where(part => part.HasBuff(BuffCode.Bleeding, false)).ToArray();
			if (bleedingParts.Length == 0) continue;
			var validTargets = character.bodyParts.Where(part => part.HitPoint.value > 0).ToArray();
			if (validTargets.Length == 0) continue;
			var index = GD.RandRange(0, validTargets.Length - 1);
			events.Add((character, validTargets[index]));
		}
		if (events.Count == 0) return;
		using var _ = DialogueManager.CreateGenericDialogue(out var dialogue);
		foreach (var (character, part) in events)
		{
			part.HitPoint.value -= 1;
			var node = combatNode.GetCharacterNode(character);
			node.Shake();
			node.FlashPropertyNode(part);
			await dialogue.ShowTextTask($"{character.name}的{part.Name}因流血失去1点生命");
		}
	}
}
