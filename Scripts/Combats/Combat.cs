using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
using RealismCombat.Nodes.Games;
namespace RealismCombat.Combats;
public class Combat
{
	readonly PlayerInput playerInput;
	readonly AIInput aiInput;
	readonly CombatNode combatNode;
	readonly TaskCompletionSource taskCompletionSource = new();
	public double Time { get; private set; }
	public Character? Considering { get; private set; }
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
	public CharacterNode? TryGetCharacterNode(Character character) => combatNode.TryGetCharacterNode(character);
	async void StartLoop()
	{
		try
		{
			var dialogue = DialogueManager.CreateGenericDialogue("战斗开始了!");
			await dialogue;
			while (true)
			{
				if (CheckBattleOutcome()) break;
				foreach (var character in Allies.Union(Enemies).Where(c => c.IsAlive))
				{
					var action = character.combatAction;
					if (action is not null)
						if (!await action.UpdateTask())
							character.combatAction = null;
				}
				while (TryGetActor(out var actor))
				{
					Log.Print($"{actor.name}的回合!");
					CombatInput input = Allies.Contains(actor) ? playerInput : aiInput;
					Considering = actor;
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
				foreach (var character in Allies.Union(Enemies).Where(c => c.IsAlive)) character.actionPoint.value += character.speed.value * 0.1f;
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
		var result = Allies.Union(Enemies).Where(c => c.IsAlive).FirstOrDefault(c => c.actionPoint.value >= c.actionPoint.maxValue);
		actor = result!;
		return result != null;
	}
	bool CheckBattleOutcome()
	{
		if (!Allies.Any(c => c.IsAlive))
		{
			Log.Print("战斗失败");
			taskCompletionSource.TrySetResult();
			return true;
		}
		if (!Enemies.Any(c => c.IsAlive))
		{
			Log.Print("敌人被消灭，你胜利了");
			taskCompletionSource.TrySetResult();
			return true;
		}
		return false;
	}
}
