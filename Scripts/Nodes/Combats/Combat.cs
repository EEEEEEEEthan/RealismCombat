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
	readonly Character[] allies;
	readonly Character[] enemies;
	readonly PlayerInput playerInput = new();
	readonly AIInput aiInput = new();
	readonly TaskCompletionSource taskCompletionSource = new();
	public Combat(Character[] allies, Character[] enemies)
	{
		this.allies = allies;
		this.enemies = enemies;
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
				Log.Print($"第{ticks}个tick");
				++ticks;
				if (ticks >= 32)
				{
					Log.Print("敌人逃走了，你胜利了");
					taskCompletionSource.SetResult();
					break;
				}
				while (TryGetActor(out var actor))
					if (allies.Contains(actor))
					{
						var action = await playerInput.MakeDecisionTask();
						await action.ExecuteTask();
					}
					else
					{
						var action = await aiInput.MakeDecisionTask();
						await action.ExecuteTask();
					}
				await Task.Delay(1000);
				foreach (var character in allies.Union(enemies)) character.actionPoint.value += character.speed.value;
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
	bool TryGetActor(out Character actor)
	{
		var result = allies.Union(enemies).FirstOrDefault(c => c.actionPoint.value >= c.actionPoint.maxValue);
		actor = result!;
		return result != null;
	}
}
