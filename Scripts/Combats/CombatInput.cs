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
	protected Combat CurrentCombat => combat;
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
	protected ICombatTarget[] GetAliveTargets(Character character) => character.bodyParts.Where(part => part.IsTargetAlive).Cast<ICombatTarget>().ToArray();
}
public class PlayerInput(Combat combat) : CombatInput(combat)
{
	public override async Task<CombatAction> MakeDecisionTask(Character character)
	{
		while (true)
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
			while (true)
			{
				var menu = DialogueManager.CreateMenuDialogue(true, options);
				var selected = await menu;
				if (selected == aliveOpponents.Length) break;
				var selectedOpponent = aliveOpponents[selected];
				while (true)
				{
					var aliveTargets = GetAliveTargets(selectedOpponent);
					if (aliveTargets.Length == 0) throw new InvalidOperationException("未找到可攻击部位");
					var targetOptions = aliveTargets
						.Select(o => new MenuOption
						{
							title = $"{selectedOpponent.name}的{o.TargetName}",
							description = $"生命 {o.HitPoint.value}/{o.HitPoint.maxValue}",
						})
						.ToArray();
					var targetMenu = DialogueManager.CreateMenuDialogue(true, targetOptions);
					var targetIndex = await targetMenu;
					if (targetIndex == aliveTargets.Length) break;
					return new Attack(character, selectedOpponent, aliveTargets[targetIndex], CurrentCombat);
				}
			}
		}
	}
}
public class AIInput(Combat combat) : CombatInput(combat)
{
	public override Task<CombatAction> MakeDecisionTask(Character character)
	{
		var target = GetRandomOpponent(character);
		if (target == null) throw new InvalidOperationException("未找到可攻击目标");
		var aliveTargets = GetAliveTargets(target);
		if (aliveTargets.Length == 0) throw new InvalidOperationException("未找到可攻击部位");
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)aliveTargets.Length);
		return Task.FromResult<CombatAction>(new Attack(character, target, aliveTargets[index], CurrentCombat));
	}
}
