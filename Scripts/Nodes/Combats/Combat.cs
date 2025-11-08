using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Combats;
public partial class Combat : Node
{
	readonly PlayerInput playerInput;
	readonly AIInput aiInput;
	readonly TaskCompletionSource taskCompletionSource = new();
	internal Character[] Allies { get; }
	internal Character[] Enemies { get; }
	public Combat(Character[] allies, Character[] enemies)
	{
		this.Allies = allies;
		this.Enemies = enemies;
		playerInput = new(this);
		aiInput = new(this);
		StartLoop();
	}
	Combat() { }
	public TaskAwaiter GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
	async void StartLoop()
	{
		try
		{
			var dialogue = DialogueManager.CreateGenericDialogue("战斗开始了!");
			await dialogue;
			var ticks = 0;
			while (this.Valid())
			{
				if (CheckBattleOutcome()) break;
				Log.Print($"第{ticks}个tick");
				++ticks;
				if (ticks >= 32)
				{
					Log.Print("敌人逃走了，你胜利了");
					taskCompletionSource.SetResult();
					break;
				}
				while (TryGetActor(out var actor))
				{
					CombatInput input = Allies.Contains(actor) ? playerInput : aiInput;
					var action = await input.MakeDecisionTask(actor);
					await action.ExecuteTask();
					if (CheckBattleOutcome()) break;
				}
				if (CheckBattleOutcome()) break;
				await Task.Delay(1000);
				foreach (var character in Allies.Union(Enemies).Where(c => c.IsAlive)) character.actionPoint.value += character.speed.value;
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
