using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.Combats;
public abstract class CombatInput(Combat combat)
{
	public abstract Task<CombatAction> MakeDecisionTask(Character character);
	protected Character[] GetOpponents(Character character) => combat.Allies.Contains(character) ? combat.Enemies : combat.Allies;
	protected Character[] GetAliveOpponents(Character character) => GetOpponents(character).Where(c => c.IsAlive).ToArray();
	protected Character? GetRandomOpponent(Character character)
	{
		var alive = GetAliveOpponents(character);
		if (alive.Length == 0) return null;
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)alive.Length);
		return alive[index];
	}
}
public class PlayerInput(Combat combat) : CombatInput(combat)
{
	public override async Task<CombatAction> MakeDecisionTask(Character character)
	{
		await DialogueManager.CreateMenuDialogue(
			new MenuOption { title = "攻击", description = "攻击敌人", }
		);
		var aliveOpponents = GetAliveOpponents(character);
		if (aliveOpponents.Length == 0) throw new InvalidOperationException("未找到可攻击目标");
		var options = aliveOpponents
			.Select(o => new MenuOption
			{
				title = o.name,
				description = $"生命 {o.hp.value}/{o.hp.maxValue}",
			})
			.ToArray();
		var menu = DialogueManager.CreateMenuDialogue(options);
		var selected = await menu;
		return new Attack(combat, character, aliveOpponents[selected]);
	}
}
public class AIInput(Combat combat) : CombatInput(combat)
{
	public override Task<CombatAction> MakeDecisionTask(Character character)
	{
		var target = GetRandomOpponent(character);
		if (target == null) throw new InvalidOperationException("未找到可攻击目标");
		return Task.FromResult<CombatAction>(new Attack(combat, character, target));
	}
}
