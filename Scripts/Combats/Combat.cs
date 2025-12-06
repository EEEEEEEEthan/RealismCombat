	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;
public class Combat
{
	public readonly CombatNode combatNode;
	public readonly HashSet<Item> droppedItems = [];
	readonly PlayerInput playerInput;
	readonly AIInput aiInput;
	readonly TaskCompletionSource taskCompletionSource = new();
	public double Time { get; private set; }
	public Character? Considering { get; private set; }
	IEnumerable<Character> AllFighters => Allies.Union(Enemies);
	internal Character[] Allies { get; }
	internal Character[] Enemies { get; }
	public Combat(Character[] allies, Character[] enemies, CombatNode combatNode)
	{
		Allies = allies;
		Enemies = enemies;
		this.combatNode = combatNode;
		combatNode.Initialize(this);
		playerInput = new(this);
		aiInput = new(this);
		StartLoop();
	}
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	internal async Task<ReactionDecision> HandleIncomingAttack(AttackBase attack)
	{
		var defender = attack.Target;
		if (defender.reaction <= 0) return ReactionDecision.CreateEndure();
		var attacker = attack.Actor;
		var target = attack.CombatTarget;
		CombatInput input = Allies.Contains(defender) ? playerInput : aiInput;
		var decision = await input.MakeReactionDecisionTask(defender, attacker, target);
		switch (decision.Type)
		{
			case ReactionType.Block when decision.BlockTarget is { Available: true, }:
				defender.reaction = Math.Max(0, defender.reaction - 1);
				return decision;
			case ReactionType.Dodge:
				defender.reaction = Math.Max(0, defender.reaction - 1);
				defender.combatAction = null;
				return decision;
			default:
				return ReactionDecision.CreateEndure();
		}
	}
	async void StartLoop()
	{
		try
		{
			await DialogueManager.ShowGenericDialogue("战斗开始了!");
			while (true)
			{
				if (CheckBattleOutcome()) break;
				foreach (var character in AllFighters.Where(c => c.IsAlive))
				{
					var action = character.combatAction;
					if (action is not null)
						if (!await action.UpdateTask())
							character.combatAction = null;
				}
				while (TryGetActor(out var actor))
				{
					actor.reaction = 1;
					var actorNode = combatNode.GetCharacterNode(actor);
					using var _ = actorNode.MoveScope(combatNode.GetReadyPosition(actor));
					using var __ = actorNode.ExpandScope();
					Considering = actor;
					await DialogueManager.ShowGenericDialogue($"{actor.name}的回合!");
					CombatInput input = Allies.Contains(actor) ? playerInput : aiInput;
					var action = await input.MakeDecisionTask(actor);
					Considering = null;
					actor.combatAction = action;
					await action.StartTask();
					if (CheckBattleOutcome()) break;
				}
				if (CheckBattleOutcome()) break;
				await Task.Delay(100);
				Time += 0.1;
				Log.Print($"{nameof(Time)}={Time:F1}");
				foreach (var character in AllFighters.Where(c => c.IsAlive)) character.actionPoint.value += character.speed.value * 0.1f;
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
			taskCompletionSource.TrySetResult();
		}
	}
	bool TryGetActor(out Character actor)
	{
		var result = AllFighters.Where(c => c.IsAlive).FirstOrDefault(c => c.actionPoint.value >= c.actionPoint.maxValue);
		actor = result!;
		return result != null;
	}
	bool CheckBattleOutcome()
	{
		if (!Allies.Any(c => c.IsAlive))
		{
			Log.Print("战斗失败");
			ClearAllBuffs();
			taskCompletionSource.TrySetResult();
			return true;
		}
		if (!Enemies.Any(c => c.IsAlive))
		{
			Log.Print("敌人被消灭，你胜利了");
			ClearAllBuffs();
			taskCompletionSource.TrySetResult();
			return true;
		}
		return false;
	}
	void ClearAllBuffs()
	{
		foreach (var character in AllFighters)
		{
			ClearCharacterBuffs(character);
		}
	}
	static void ClearCharacterBuffs(Character character)
	{
		foreach (var bodyPart in character.bodyParts)
		{
			if (bodyPart is IBuffOwner bodyPartBuffOwner)
			{
				ClearBuffs(bodyPartBuffOwner);
			}
			foreach (var slot in bodyPart.Slots)
			{
				if (slot.Item != null)
				{
					ClearItemBuffs(slot.Item);
				}
			}
		}
		foreach (var item in character.inventory.Items)
		{
			ClearItemBuffs(item);
		}
	}
	static void ClearItemBuffs(Item item)
	{
		if (item is IBuffOwner buffOwner)
		{
			ClearBuffs(buffOwner);
		}
		foreach (var slot in item.Slots)
		{
			if (slot.Item != null)
			{
				ClearItemBuffs(slot.Item);
			}
		}
	}
	static void ClearBuffs(IBuffOwner buffOwner)
	{
		var buffsToRemove = new List<Buff>(buffOwner.Buffs);
		foreach (var buff in buffsToRemove)
		{
			buffOwner.RemoveBuff(buff);
		}
	}
}
