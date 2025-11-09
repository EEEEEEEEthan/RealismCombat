using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
namespace RealismCombat.Combats;
public class Combat
{
	readonly PlayerInput playerInput;
	readonly AIInput aiInput;
	readonly TaskCompletionSource taskCompletionSource = new();
	public double Time { get; private set; }
	internal Character[] Allies { get; }
	internal Character[] Enemies { get; }
	public Combat(Character[] allies, Character[] enemies)
	{
		Allies = allies;
		Enemies = enemies;
		playerInput = new(this);
		aiInput = new(this);
		StartLoop();
	}
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	async void StartLoop()
	{
		try
		{
			var dialogue = DialogueManager.CreateGenericDialogue("战斗开始了!");
			await dialogue;
			var ticks = 0;
			while (true)
			{
				if (CheckBattleOutcome()) break;
				++ticks;
				if (ticks >= 32)
				{
					Log.Print("敌人逃走了，你胜利了");
					taskCompletionSource.SetResult();
					break;
				}
				foreach (var character in Allies.Union(Enemies).Where(c => c.IsAlive))
				{
					var action = character.combatAction;
					if (action is not null)
						if (!await action.UpdateTask())
							character.combatAction = null;
				}
				while (TryGetActor(out var actor))
				{
					CombatInput input = Allies.Contains(actor) ? playerInput : aiInput;
					var action = await input.MakeDecisionTask(actor);
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
