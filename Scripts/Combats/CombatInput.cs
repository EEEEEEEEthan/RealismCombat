using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
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
	protected static string BuildTargetDescription(ICombatTarget target)
	{
		var description = $"生命 {target.HitPoint.value}/{target.HitPoint.maxValue}";
		if (target is IBuffOwner { Buffs.Count: > 0, } buffOwner)
		{
			var buffLines = buffOwner.Buffs
				.Select(buff => $"[{buff.code.GetName()}]来自{buff.source?.name ?? "未知"}");
			description += "\n" + string.Join("\n", buffLines);
		}
		return description;
	}
	protected readonly Combat combat = combat;
	public abstract Task<CombatAction> MakeDecisionTask(Character character);
	public virtual Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	) =>
		Task.FromResult(ReactionDecision.CreateEndure());
	protected List<(string name, CombatAction action)> BuildActions(Character actor, BodyPart bodyPart)
	{
		var actions = new List<(string, CombatAction)>();
		void TryAdd(CombatActionCode code, string name, Func<CombatAction?> factory)
		{
			if (!actor.availableCombatActions.ContainsKey(code)) return;
			var action = factory();
			if (action == null) return;
			if (!action.Available) return;
			actions.Add((name, action));
		}
		TryAdd(CombatActionCode.Slash, "斩击", () => new SlashAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Stab, "刺击", () => new StabAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Kick, "踢", () => new KickAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Headbutt, "头槌", () => new HeadbuttAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Charge, "撞击", () => new ChargeAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Grab, "抓", () => new GrabAttack(actor, bodyPart, combat));
		TryAdd(CombatActionCode.BreakFree, "抽出", () => new BreakFreeAction(actor, bodyPart, combat));
		TryAdd(CombatActionCode.Release, "放手", () => new ReleaseAction(actor, bodyPart, combat));
		TryAdd(CombatActionCode.TakeWeapon, "拿", () => new TakeWeaponAction(actor, bodyPart, combat));
		TryAdd(CombatActionCode.PickWeapon, "捡", () => new PickWeaponAction(actor, bodyPart, combat));
		return actions;
	}
}
public class PlayerInput(Combat combat) : CombatInput(combat)
{
	public override async Task<CombatAction> MakeDecisionTask(Character character)
	{
		string FormatChance(double value) => $"{Math.Round(value * 100)}%";
		string FormatDamage(Damage damage)
		{
			static string FormatValue(float value) => value.ToString("0.##");
			var parts = new[]
			{
				$"砍{FormatValue(damage.slash)}",
				$"刺{FormatValue(damage.pierce)}",
				$"钝{FormatValue(damage.blunt)}",
			};
			return $"伤害 {string.Join(" ", parts)}";
		}
		while (true)
		{
			var bodyPartActions = GetAvailableTargets(character)
				.Cast<BodyPart>()
				.Select(bp => (bodyPart: bp, actions: BuildActions(character, bp)))
				.Where(pair => pair.actions.Count > 0)
				.ToArray();
			if (bodyPartActions.Length == 0) throw new InvalidOperationException("未找到可用的身体部位");
			var turnTitle = $"{character.name}的回合";
			var bodyPartOptions = bodyPartActions
				.Select(pair =>
				{
					var bp = pair.bodyPart;
					return new MenuOption
					{
						title = bp.GetNameWithEquipments(),
						description = BuildTargetDescription(bp),
					};
				})
				.ToArray();
			var bodyPartMenu = DialogueManager.CreateMenuDialogue(turnTitle, true, bodyPartOptions);
			var bodyPartIndex = await bodyPartMenu;
			if (bodyPartIndex == bodyPartOptions.Length) continue;
			var selectedBodyPart = bodyPartActions[bodyPartIndex].bodyPart;
			var bodyPartTitle = $"{character.name}的{selectedBodyPart.Name}";
			while (true)
			{
				var actions = BuildActions(character, selectedBodyPart);
				if (actions.Count == 0)
				{
					await DialogueManager.ShowGenericDialogue($"{selectedBodyPart.Name}无法使用任何行动");
					break;
				}
				var actionOptions = actions
					.Select(a => new MenuOption
					{
						title = a.name,
						description = a.action is AttackBase attack
							? $"{FormatDamage(attack.GetPreviewDamage())}\n{a.action.Description}"
							: a.action.Description,
						disabled = a.action.Disabled,
					})
					.ToArray();
				var actionMenu = DialogueManager.CreateMenuDialogue(bodyPartTitle, true, actionOptions);
				var actionIndex = await actionMenu;
				if (actionIndex == actionOptions.Length) break;
				var selected = actions[actionIndex];
				if (!selected.action.CanUse)
				{
					await DialogueManager.ShowGenericDialogue("行动无法执行");
					continue;
				}
				var actionTitle = $"{bodyPartTitle}{selected.name}";
				var prepared = await PrepareAction(selected.action, FormatChance, actionTitle);
				if (prepared != null) return prepared;
			}
		}
	}
	public override async Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	)
	{
		var reactionAvailable = defender.reaction > 0;
		while (true)
		{
			var attack = attacker.combatAction as AttackBase;
			var attackerText = $"{attacker.name}的攻击";
			if (attack != null)
			{
				if (attack.UsesWeapon)
				{
					var weaponName = attack.ActorBodyPart.Slots
						.Select(slot => slot.Item)
						.FirstOrDefault(item => item != null && (item.flag & ItemFlagCode.Arm) != 0)
						?.Name;
					if (!string.IsNullOrEmpty(weaponName)) attackerText = $"{attacker.name}{weaponName}";
					else attackerText = $"{attacker.name}{attack.ActorBodyPart.Name}";
				}
				else
				{
					attackerText = $"{attacker.name}{attack.ActorBodyPart.Name}";
				}
			}
			var defenderText = target is BodyPart bodyPart
				? $"{defender.name}{bodyPart.GetNameWithEquipments()}"
				: $"{defender.name}{target.Name}";
			var attackName = attack?.Code switch
			{
				CombatActionCode.Slash => "斩击",
				CombatActionCode.Stab => "刺击",
				CombatActionCode.Kick => "踢",
				CombatActionCode.Headbutt => "头槌",
				CombatActionCode.Charge => "撞击",
				CombatActionCode.Grab => "抓",
				_ => "攻击",
			};
			var menuTitle = $"{attackerText}对{defenderText}的{attackName}";
			var menu = DialogueManager.CreateMenuDialogue(
				menuTitle,
				new MenuOption
				{
					title = "格挡",
					description = "消耗1点反应, 选择身体或武器承受伤害",
					disabled = !reactionAvailable,
				},
				new MenuOption
				{
					title = "闪避",
					description = "消耗1点反应, 打断自身行动并躲开伤害",
					disabled = !reactionAvailable,
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
					if (!reactionAvailable) continue;
					var blockTargets = GetBlockTargets(defender);
					if (blockTargets.Length == 0)
					{
						await DialogueManager.ShowGenericDialogue($"{defender.name}没有可用的格挡目标");
						continue;
					}
					var options = blockTargets
						.Select(t =>
						{
							return new MenuOption
							{
								title = t is BodyPart bodyPart ? bodyPart.GetNameWithEquipments() : t.Name,
								description = BuildTargetDescription(t),
							};
						})
						.ToArray();
					var blockMenu = DialogueManager.CreateMenuDialogue("选择格挡目标", true, options);
					var blockIndex = await blockMenu;
					if (blockIndex == options.Length) continue;
					return ReactionDecision.CreateBlock(blockTargets[blockIndex]);
				}
				case 1:
					if (!reactionAvailable) continue;
					return ReactionDecision.CreateDodge();
				default:
					return ReactionDecision.CreateEndure();
			}
		}
	}
	async Task<CombatAction?> PrepareAction(
		CombatAction action,
		Func<double, string> formatChance,
		string navigationTitle
	)
	{
		switch (action)
		{
			case AttackBase attack:
				return await PrepareAttackAction(attack, formatChance, navigationTitle);
			case TakeWeaponAction takeWeaponAction:
			{
				var prepared = await takeWeaponAction.PrepareByPlayerSelection();
				if (!prepared)
				{
					await DialogueManager.ShowGenericDialogue("没有可用的腰带武器");
					return null;
				}
				return takeWeaponAction;
			}
			case PickWeaponAction pickWeaponAction:
			{
				var prepared = await pickWeaponAction.PrepareByPlayerSelection();
				if (!prepared)
				{
					await DialogueManager.ShowGenericDialogue("没有可以捡起的武器");
					return null;
				}
				return pickWeaponAction;
			}
			default:
				return action;
		}
	}
	async Task<CombatAction?> PrepareAttackAction(
		AttackBase attack,
		Func<double, string> formatChance,
		string navigationTitle
	)
	{
		while (true)
		{
			var availableTargets = attack.AvailableTargets.ToArray();
			if (availableTargets.Length == 0) throw new InvalidOperationException("未找到可攻击目标");
			Character target;
			if (availableTargets.Length == 1)
			{
				target = availableTargets[0];
			}
			else
			{
				var options = availableTargets
					.Select(o => new MenuOption
					{
						title = o.name,
						description = string.Empty,
					})
					.ToArray();
				var menu = DialogueManager.CreateMenuDialogue(navigationTitle, true, options);
				var selected = await menu;
				if (selected == options.Length) return null;
				target = availableTargets[selected];
			}
			attack.Target = target;
			var targetObject = await SelectTargetObject(attack, target, formatChance, navigationTitle);
			if (targetObject == null)
			{
				if (availableTargets.Length == 1) return null;
				continue;
			}
			attack.TargetObject = targetObject;
			return attack;
		}
	}
	async Task<ICombatTarget?> SelectTargetObject(
		AttackBase attack,
		Character opponent,
		Func<double, string> formatChance,
		string navigationTitle
	)
	{
		while (true)
		{
			var availableTargets = attack.AvailableTargetObjects.ToArray();
			if (availableTargets.Length == 0) throw new InvalidOperationException("未找到可攻击部位");
			var options = availableTargets
				.Select(tuple =>
				{
					var target = tuple.target;
					attack.TargetObject = target;
					var description = BuildTargetDescription(target);
					var reactionChance = ReactionSuccessCalculator.Calculate(attack);
					description += $"\n闪避成功率 {formatChance(reactionChance.DodgeChance)}";
					description += $"\n格挡成功率 {formatChance(reactionChance.BlockChance)}";
					return new MenuOption
					{
						title = target is BodyPart bodyPart ? bodyPart.GetNameWithEquipments() : target.Name,
						description = description,
						disabled = tuple.disabled,
					};
				})
				.ToArray();
			var menuTitle = $"{navigationTitle}{opponent.name}的";
			var menu = DialogueManager.CreateMenuDialogue(menuTitle, true, options);
			var selectedIndex = await menu;
			if (selectedIndex == options.Length) return null;
			var selected = availableTargets[selectedIndex];
			if (selected.disabled) continue;
			return selected.target;
		}
	}
}
public class AIInput(Combat combat) : CombatInput(combat)
{
	public override Task<CombatAction> MakeDecisionTask(Character character)
	{
		var availableBodyParts = GetAvailableTargets(character).Cast<BodyPart>().ToArray();
		if (availableBodyParts.Length == 0) throw new InvalidOperationException("未找到可用的身体部位");
		var randomizedBodyParts = availableBodyParts.OrderBy(_ => GD.Randi()).ToArray();
		foreach (var selectedBodyPart in randomizedBodyParts)
		{
			var actions = BuildActions(character, selectedBodyPart)
				.Select(a => a.action)
				.Where(a => a.CanUse)
				.OrderBy(_ => GD.Randi())
				.ToList();
			foreach (var action in actions)
			{
				if (!TryPrepareAIAction(action)) continue;
				return Task.FromResult(action);
			}
		}
		throw new InvalidOperationException("未找到可用的行动");
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
		var itemTargets = blockTargets.Where(t => t is Item).ToArray();
		var candidateTargets = itemTargets.Length > 0 ? itemTargets : blockTargets;
		var randomValue = GD.Randi();
		var index = (int)(randomValue % (uint)candidateTargets.Length);
		return Task.FromResult(ReactionDecision.CreateBlock(candidateTargets[index]));
	}
	bool TryPrepareAIAction(CombatAction action)
	{
		return action switch
		{
			AttackBase attack => TryAssignRandomTargets(attack),
			TakeWeaponAction takeWeaponAction => takeWeaponAction.PrepareByAI(),
			PickWeaponAction pickWeaponAction => pickWeaponAction.PrepareByAI(),
			ReleaseAction { WillOnlyDropWeapon: true, } => false,
			_ => true,
		};
	}
	bool TryAssignRandomTargets(AttackBase attack)
	{
		var targets = attack.AvailableTargets.ToArray();
		if (targets.Length == 0) return false;
		var targetIndex = (int)(GD.Randi() % (uint)targets.Length);
		var target = targets[targetIndex];
		attack.Target = target;
		var targetObjects = attack.AvailableTargetObjects.Where(t => !t.disabled).ToArray();
		if (targetObjects.Length == 0) return false;
		var objectIndex = (int)(GD.Randi() % (uint)targetObjects.Length);
		attack.TargetObject = targetObjects[objectIndex].target;
		return true;
	}
}
