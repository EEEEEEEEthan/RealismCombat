using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Characters;
using RealismCombat.Combats.CombatActions;
using RealismCombat.Items;
using RealismCombat.Nodes.Dialogues;
using RealismCombat.Nodes.Games;
namespace RealismCombat.Combats;
public abstract class CombatInput(Combat combat)
{
	protected static ICombatTarget[] GetAvailableTargets(Character character) =>
		character.bodyParts.Where(part => part.Available).Cast<ICombatTarget>().ToArray();
	protected static ICombatTarget[] GetBlockTargets(Character character)
	{
		var targets = new List<ICombatTarget>();
		foreach (var bodyPart in character.bodyParts)
		{
			if (bodyPart.Available) targets.Add(bodyPart);
			if (bodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm)
				foreach (var slot in bodyPart.Slots)
					if (slot.Item is { Available: true, } target)
						targets.Add(target);
		}
		return targets.ToArray();
	}
	/// <summary>
	///     获取可用的攻击类型
	/// </summary>
	protected static List<(string name, Func<Character, BodyPart, Character, ICombatTarget, Combat, AttackBase> create)> GetAvailableAttacks(BodyPart bodyPart)
	{
		var attacks = new List<(string, Func<Character, BodyPart, Character, ICombatTarget, Combat, AttackBase>)>();
		if (SlashAttack.CanUse(bodyPart)) attacks.Add(("斩击", (a, b, t, c, co) => new SlashAttack(a, b, t, c, co)));
		if (StabAttack.CanUse(bodyPart)) attacks.Add(("刺击", (a, b, t, c, co) => new StabAttack(a, b, t, c, co)));
		if (KickAttack.CanUse(bodyPart)) attacks.Add(("踢", (a, b, t, c, co) => new KickAttack(a, b, t, c, co)));
		if (HeadbuttAttack.CanUse(bodyPart)) attacks.Add(("头槌", (a, b, t, c, co) => new HeadbuttAttack(a, b, t, c, co)));
		if (ChargeAttack.CanUse(bodyPart)) attacks.Add(("撞击", (a, b, t, c, co) => new ChargeAttack(a, b, t, c, co)));
		if (GrabAttack.CanUse(bodyPart)) attacks.Add(("抓取", (a, b, t, c, co) => new GrabAttack(a, b, t, c, co)));
		return attacks;
	}
	protected readonly Combat combat = combat;
	public abstract Task<CombatAction> MakeDecisionTask(Character character);
	public virtual Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	) =>
		Task.FromResult(ReactionDecision.CreateEndure());
	protected Character[] GetAliveOpponents(Character character) => GetOpponents(character).Where(c => c.IsAlive).ToArray();
	protected Character? GetRandomOpponent(Character character)
	{
		var alive = GetAliveOpponents(character);
		if (alive.Length == 0) return null;
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)alive.Length);
		return alive[index];
	}
	protected CharacterNode GetCharacterNode(Character character) => combat.combatNode.GetCharacterNode(character);
	Character[] GetOpponents(Character character) => combat.Allies.Contains(character) ? combat.Enemies : combat.Allies;
}
public class PlayerInput(Combat combat) : CombatInput(combat)
{
	public override async Task<CombatAction> MakeDecisionTask(Character character)
	{
		while (true)
		{
			var availableBodyParts = GetAvailableTargets(character).Cast<BodyPart>().ToArray();
			if (availableBodyParts.Length == 0) throw new InvalidOperationException("未找到可用的身体部位");
			var bodyPartOptions = availableBodyParts
				.Select(bp =>
				{
					var description = $"生命 {bp.HitPoint.value}/{bp.HitPoint.maxValue}";
					if (bp.Buffs.Count > 0)
					{
						var buffLines = bp.Buffs
							.Select(buff => $"[{buff.code.GetName()}]来自{buff.source?.name ?? "未知"}");
						description += "\n" + string.Join("\n", buffLines);
					}
					return new MenuOption
					{
						title = bp.Name,
						description = description,
					};
				})
				.ToArray();
			while (true)
			{
				var bodyPartMenu = DialogueManager.CreateMenuDialogue(true, bodyPartOptions);
				var bodyPartIndex = await bodyPartMenu;
				if (bodyPartIndex == availableBodyParts.Length) break;
				var selectedBodyPart = availableBodyParts[bodyPartIndex];
				var availableAttacks = GetAvailableAttacks(selectedBodyPart);
				if (availableAttacks.Count == 0)
				{
					var tip = DialogueManager.CreateGenericDialogue($"{character.name}的{selectedBodyPart.Name}无法使用任何攻击");
					await tip;
					continue;
				}
				var attackOptions = availableAttacks
					.Select(a => new MenuOption
					{
						title = a.name,
						description = string.Empty,
					})
					.ToArray();
				var attackMenu = DialogueManager.CreateMenuDialogue(true, attackOptions);
				var attackIndex = await attackMenu;
				if (attackIndex == availableAttacks.Count) continue;
				var selectedAttack = availableAttacks[attackIndex];
				var aliveOpponents = GetAliveOpponents(character);
				if (aliveOpponents.Length == 0) throw new InvalidOperationException("未找到可攻击目标");
				var options = aliveOpponents
					.Select(o => new MenuOption
					{
						title = o.name,
						description = string.Empty,
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
						var aliveTargets = GetAvailableTargets(selectedOpponent);
						if (aliveTargets.Length == 0) throw new InvalidOperationException("未找到可攻击部位");
						var targetOptions = aliveTargets
							.Select(o =>
							{
								var description = $"生命 {o.HitPoint.value}/{o.HitPoint.maxValue}";
								if (o is IBuffOwner buffOwner && buffOwner.Buffs.Count > 0)
								{
									var buffLines = buffOwner.Buffs
										.Select(buff => $"[{buff.code.GetName()}]来自{buff.source?.name ?? "未知"}");
									description += "\n" + string.Join("\n", buffLines);
								}
								return new MenuOption
								{
									title = o.Name,
									description = description,
								};
							})
							.ToArray();
						var targetMenu = DialogueManager.CreateMenuDialogue(true, targetOptions);
						var targetIndex = await targetMenu;
						if (targetIndex == aliveTargets.Length) break;
						return selectedAttack.create(character, selectedBodyPart, selectedOpponent, aliveTargets[targetIndex], combat);
					}
				}
			}
		}
	}
	public override async Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	)
	{
		if (defender.reaction <= 0) return ReactionDecision.CreateEndure();
		while (true)
		{
			var menu = DialogueManager.CreateMenuDialogue(
				new MenuOption
				{
					title = "格挡",
					description = "消耗1点反应, 选择身体或武器承受伤害",
				},
				new MenuOption
				{
					title = "闪避",
					description = "消耗1点反应, 打断自身行动并躲开伤害",
				},
				new MenuOption
				{
					title = "承受",
					description = "不进行额外反应",
				}
			);
			var selected = await menu;
			switch (selected)
			{
				case 0:
				{
					var blockTargets = GetBlockTargets(defender);
					if (blockTargets.Length == 0)
					{
						var tip = DialogueManager.CreateGenericDialogue($"{defender.name}没有可用的格挡目标");
						await tip;
						continue;
					}
					var options = blockTargets
						.Select(t =>
						{
							var description = $"生命 {t.HitPoint.value}/{t.HitPoint.maxValue}";
							if (t is IBuffOwner buffOwner && buffOwner.Buffs.Count > 0)
							{
								var buffLines = buffOwner.Buffs
									.Select(buff => $"[{buff.code.GetName()}]来自{buff.source?.name ?? "未知"}");
								description += "\n" + string.Join("\n", buffLines);
							}
							return new MenuOption
							{
								title = t.Name,
								description = description,
							};
						})
						.ToArray();
					var blockMenu = DialogueManager.CreateMenuDialogue(true, options);
					var blockIndex = await blockMenu;
					if (blockIndex == options.Length) continue;
					return ReactionDecision.CreateBlock(blockTargets[blockIndex]);
				}
				case 1:
					return ReactionDecision.CreateDodge();
				default:
					return ReactionDecision.CreateEndure();
			}
		}
	}
}
public class AIInput(Combat combat) : CombatInput(combat)
{
	public override Task<CombatAction> MakeDecisionTask(Character character)
	{
		var availableBodyParts = GetAvailableTargets(character).Cast<BodyPart>().ToArray();
		if (availableBodyParts.Length == 0) throw new InvalidOperationException("未找到可用的身体部位");
		var bodyPartRandomValue = GD.Randi();
		var bodyPartIndex = (int)(bodyPartRandomValue % (uint)availableBodyParts.Length);
		var selectedBodyPart = availableBodyParts[bodyPartIndex];
		var availableAttacks = GetAvailableAttacks(selectedBodyPart);
		if (availableAttacks.Count == 0)
		{
			bodyPartIndex = (bodyPartIndex + 1) % availableBodyParts.Length;
			selectedBodyPart = availableBodyParts[bodyPartIndex];
			availableAttacks = GetAvailableAttacks(selectedBodyPart);
			if (availableAttacks.Count == 0) throw new InvalidOperationException("未找到可用的攻击类型");
		}
		var attackRandomValue = GD.Randi();
		var attackIndex = (int)(attackRandomValue % (uint)availableAttacks.Count);
		var selectedAttack = availableAttacks[attackIndex];
		var target = GetRandomOpponent(character);
		if (target == null) throw new InvalidOperationException("未找到可攻击目标");
		var aliveTargets = GetAvailableTargets(target);
		if (aliveTargets.Length == 0) throw new InvalidOperationException("未找到可攻击部位");
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)aliveTargets.Length);
		return Task.FromResult<CombatAction>(selectedAttack.create(character, selectedBodyPart, target, aliveTargets[index], combat));
	}
	public override Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	)
	{
		if (defender.reaction <= 0) return Task.FromResult(ReactionDecision.CreateEndure());
		var blockTargets = GetBlockTargets(defender);
		if (blockTargets.Length == 0) return Task.FromResult(ReactionDecision.CreateEndure());
		var priorityTarget = blockTargets.FirstOrDefault(t => t is Item);
		var chosen = priorityTarget ?? blockTargets.First();
		return Task.FromResult(ReactionDecision.CreateBlock(chosen));
	}
}
