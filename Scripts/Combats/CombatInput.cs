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
			if (bodyPart.Free) targets.Add(bodyPart);
			if (bodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm)
			{
				if (!bodyPart.Free) continue;
				foreach (var slot in bodyPart.Slots)
					if (slot.Item is { } target && target.Free)
						targets.Add(target);
			}
		}
		return targets.ToArray();
	}
	protected static string BuildTargetDescription(ICombatTarget target)
	{
		var description = $"生命 {target.HitPoint.value}/{target.HitPoint.maxValue}";
		if (target is IBuffOwner { Buffs.Count: > 0, } buffOwner)
		{
			static string FormatBuffSource(Buff buff)
			{
				if (buff.source is not { } source) return "未知";
				var characterName = source.Character.name;
				var targetName = source.Target.Name;
				return $"{characterName}-{targetName}";
			}
			var buffLines = buffOwner.Buffs.Values
				.Select(buff => $"[{buff.code.Name}]来自{FormatBuffSource(buff)}");
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
			if (!action.Visible) return;
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
		TryAdd(CombatActionCode.GetUp, "爬起", () => new GetUpAction(actor, bodyPart, combat));
#if DEBUG
		TryAdd(CombatActionCode.Execution, "(测试)处决", () => new ExecutionAction(actor, bodyPart, combat));
#endif
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
				$"砍{FormatValue(damage.Slash)}",
				$"刺{FormatValue(damage.Pierce)}",
				$"钝{FormatValue(damage.Blunt)}",
			};
			return $"伤害 {string.Join(" ", parts)}";
		}
		LogAllCharactersState();
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
						title = bp.NameWithEquipments,
						description = BuildTargetDescription(bp),
					};
				})
				.ToArray();
			var bodyPartMenu = DialogueManager.CreateMenuDialogue(turnTitle, bodyPartOptions);
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
					.Select(a =>
					{
						var costText = $"行动力 前摇{a.action.preCastActionPointCost} 后摇{a.action.postCastActionPointCost}";
						var description = a.action is AttackBase attack
							? $"{FormatDamage(attack.Damage)}\n{costText}\n{a.action.Description}"
							: $"{costText}\n{a.action.Description}";
						return new MenuOption
						{
							title = a.name,
							description = description,
							disabled = a.action.Disabled || a.action.DisabledByBuff,
						};
					})
					.ToArray();
				var actionMenu = DialogueManager.CreateMenuDialogue(bodyPartTitle, true, actionOptions);
				var actionIndex = await actionMenu;
				if (actionIndex == actionOptions.Length) break;
				var selected = actions[actionIndex];
				if (selected.action.DisabledByBuff || !selected.action.CanUse)
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
		string FormatChance(double value) => $"{Math.Round(value * 100)}%";
		while (true)
		{
			var attack = attacker.combatAction as AttackBase;
			var reactionChance = attack?.ReactionChance ?? new(0.0, 0.0);
			var blockChanceText = $"成功率 {FormatChance(reactionChance.BlockChance)}";
			var dodgeChanceText = $"成功率 {FormatChance(reactionChance.DodgeChance)}";
			var attackerText = $"{attacker.name}的攻击";
			if (attack != null)
			{
				if (attack.UsesWeapon)
				{
					var weaponName = attack
						.ActorBodyPart.Slots
						.Select(slot => slot.Item)
						.FirstOrDefault(item => item != null && (item.flag & ItemFlagCode.Arm) != 0)
						?.Name;
					attackerText = !string.IsNullOrEmpty(weaponName) ? $"{attacker.name}{weaponName}" : $"{attacker.name}{attack.ActorBodyPart.Name}";
				}
				else
				{
					attackerText = $"{attacker.name}{attack.ActorBodyPart.Name}";
				}
			}
			var defenderText = target is BodyPart bodyPart
				? $"{defender.name}{bodyPart.Name}"
				: $"{defender.name}{target.Name}";
			var attackName = attack?.Id switch
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
			var blockDescription = $"{blockChanceText}\n消耗1点反应, 选择身体或武器承受伤害";
			if (defender.combatAction != null)
				blockDescription += "\n使用当前行动部位格挡会打断自身行动";
			var menu = DialogueManager.CreateMenuDialogue(
				menuTitle,
				new MenuOption
				{
					title = "格挡",
					description = blockDescription,
					disabled = !reactionAvailable,
				},
				new MenuOption
				{
					title = "闪避",
					description = $"{dodgeChanceText}\n消耗1点反应, 打断自身行动并躲开伤害",
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
						.Select(t => new MenuOption
						{
							title = t is BodyPart bodyPart ? bodyPart.NameWithEquipments : t.Name,
							description = BuildTargetDescription(t),
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
	void LogAllCharactersState()
	{
		var fighters = combat.Allies.Concat(combat.Enemies);
		foreach (var fighter in fighters)
		{
			var bodyStates = fighter.bodyParts.Select(bp => $"{bp.Name}{bp.HitPoint.value}/{bp.HitPoint.maxValue}");
			Log.Print(
				$"[回合状态]{fighter.name} 行动 {fighter.actionPoint.value}/{fighter.actionPoint.maxValue} 反应 {fighter.reaction} {string.Join(" ", bodyStates)}");
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
#if DEBUG
			case ExecutionAction executionAction:
				return await PrepareExecutionAction(executionAction, navigationTitle);
#endif
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
			attack.target = target;
			var targetObject = await SelectTargetObject(attack, target, formatChance, navigationTitle);
			if (targetObject == null)
			{
				if (availableTargets.Length == 1) return null;
				continue;
			}
			attack.targetObject = targetObject;
			return attack;
		}
	}
#if DEBUG
	async Task<CombatAction?> PrepareExecutionAction(ExecutionAction action, string navigationTitle)
	{
		while (true)
		{
			var availableTargets = action.AvailableTargets.ToArray();
			if (availableTargets.Length == 0) throw new InvalidOperationException("未找到可处决的目标");
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
			action.Target = target;
			var targetObjects = target.bodyParts.ToArray();
			var options2 = targetObjects
				.Select(bp => new MenuOption
				{
					title = bp.NameWithEquipments,
					description = BuildTargetDescription(bp),
				})
				.ToArray();
			var menuTitle = $"{navigationTitle}{target.name}的";
			var menu2 = DialogueManager.CreateMenuDialogue(menuTitle, true, options2);
			var selectedIndex = await menu2;
			if (selectedIndex == options2.Length)
			{
				if (availableTargets.Length == 1) return null;
				continue;
			}
			action.TargetObject = targetObjects[selectedIndex];
			return action;
		}
	}
#endif
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
					attack.targetObject = target;
					var description = BuildTargetDescription(target);
					var reactionChance = attack.ReactionChance;
					description += $"\n闪避成功率 {formatChance(reactionChance.DodgeChance)}";
					description += $"\n格挡成功率 {formatChance(reactionChance.BlockChance)}";
					return new MenuOption
					{
						title = target is BodyPart bodyPart ? bodyPart.NameWithEquipments : target.Name,
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
public class GenericAIInput(Combat combat) : CombatInput(combat)
{
	const float reactionEndureChance = 0.25f;
	public override Task<CombatAction> MakeDecisionTask(Character character)
	{
		var actions = new Dictionary<CombatAction, double>();
		{
			// 检查是否处于倒伏状态，如果是则优先尝试爬起
			var hasProneBuff = character.bodyParts.Any(part => part.HasBuff(BuffCode.Prone, false));
			if (hasProneBuff)
			{
				foreach (var arm in character.bodyParts.Where(bp => bp.id.IsArm))
				{
					var getUpAction = new GetUpAction(character, arm, combat);
					if (getUpAction.CanUse)
					{
						actions[getUpAction] = 100d; // 倒伏时爬起的优先级最高
					}
				}
			}
			void AddAttackOptions(Func<BodyPart, AttackBase> factory)
			{
				foreach (var bodyPart in character.bodyParts)
				{
					var action = factory(bodyPart);
					foreach (var target in combat.Allies)
					{
					action.target = target;
					foreach (var targetObj in target.bodyParts)
					{
						action.targetObject = targetObj;
						if (action is ChargeAttack && targetObj.id.IsLeg) continue;
						if (action is HeadbuttAttack && targetObj.id != BodyPartCode.Head) continue;
						if (!action.CanUse) continue;
							var expected = AttackBase.CalculateExpectedBodyDamage(action.Damage, targetObj);
							var chance = action.ReactionChance;
							var weight = expected * (1 - chance.HighestChance);
							actions[action] = weight;
							action = factory(bodyPart);
							action.target = target;
							action.targetObject = targetObj;
						}
					}
				}
			}
			AddAttackOptions(bodyPart => new SlashAttack(character, bodyPart, combat));
			AddAttackOptions(bodyPart => new StabAttack(character, bodyPart, combat));
			AddAttackOptions(bodyPart => new KickAttack(character, bodyPart, combat));
			AddAttackOptions(bodyPart => new HeadbuttAttack(character, bodyPart, combat));
			AddAttackOptions(bodyPart => new ChargeAttack(character, bodyPart, combat));
			AddAttackOptions(bodyPart => new GrabAttack(character, bodyPart, combat));
			var arms = character.bodyParts.Where(bp => bp.id.IsArm).ToArray();
			if (arms.Length > 0 && arms.All(bp => !bp.HasWeapon))
				foreach (var bodyPart in arms)
				{
					var takeWeaponAction = new TakeWeaponAction(character, bodyPart, combat);
					if (!takeWeaponAction.CanUse) continue;
					actions[takeWeaponAction] = 50d;
				}
			foreach (var bodyPart in character.bodyParts.Where(bp =>
				bp.id.IsArm && (bp.HasBuff(BuffCode.Restrained, true) || bp.HasBuff(BuffCode.Grappling, true))))
			{
				var releaseAction = new ReleaseAction(character, bodyPart, combat);
				if (!releaseAction.CanUse) continue;
				actions[releaseAction] = 50d;
			}
		}
		// actions里选取权重最高的
		while (actions.Count > 0)
		{
			var best = actions
				.OrderByDescending(pair => pair.Value)
				.ThenBy(_ => GD.Randi())
				.First()
				.Key;
			if (TryPrepareAIAction(best)) return Task.FromResult(best);
			actions.Remove(best);
		}
		throw new InvalidOperationException("AI无法执行任何行动");
	}
	public override Task<ReactionDecision> MakeReactionDecisionTask(
		Character defender,
		Character attacker,
		ICombatTarget target
	)
	{
		if (defender.reaction <= 0 || GD.Randf() < reactionEndureChance || attacker.combatAction is not AttackBase attack)
			return Task.FromResult(ReactionDecision.CreateEndure());
		var reactionChance = attack.ReactionChance;
		var originalExpected = AttackBase.CalculateExpectedBodyDamage(attack.Damage, target);
		var options = new List<(ReactionDecision decision, double expectedDamage)>
		{
			(ReactionDecision.CreateDodge(), (1d - reactionChance.DodgeChance) * originalExpected),
		};
		var blockTargets = GetBlockTargets(defender);
		foreach (var blockTarget in blockTargets)
		{
			if (target is BodyPart { id: BodyPartCode.Head, } &&
				blockTarget is BodyPart { id: { IsLeg: true, }, })
				continue;
			var blockExpected = AttackBase.CalculateExpectedBodyDamage(attack.Damage, blockTarget);
			var expectedDamage = reactionChance.BlockChance * blockExpected + (1d - reactionChance.BlockChance) * originalExpected;
			// 如果格挡会打断自身行动，增加期望伤害作为惩罚
			if (defender.combatAction?.WillBeInterruptedByBlockingWith(blockTarget) == true)
				expectedDamage *= 1.5;
			options.Add((ReactionDecision.CreateBlock(blockTarget), expectedDamage));
		}
		if (options.Count == 0) return Task.FromResult(ReactionDecision.CreateEndure());
		var zeroExpected = options.Where(pair => pair.expectedDamage <= 0d).ToArray();
		if (zeroExpected.Length > 0)
		{
			var zeroIndex = (int)(GD.Randi() % (uint)zeroExpected.Length);
			return Task.FromResult(zeroExpected[zeroIndex].decision);
		}
		var totalWeight = options.Sum(pair => getWeight(pair.expectedDamage));
		if (totalWeight <= 0d || double.IsInfinity(totalWeight) || double.IsNaN(totalWeight)) return Task.FromResult(ReactionDecision.CreateEndure());
		var randomValue = GD.Randf() * totalWeight;
		foreach (var option in options)
		{
			var weight = getWeight(option.expectedDamage);
			if (randomValue <= weight) return Task.FromResult(option.decision);
			randomValue -= weight;
		}
		return Task.FromResult(options.First().decision);
		double getWeight(double expectedDamage) => expectedDamage <= 0d ? 1 : 1d / Mathf.Pow(expectedDamage, 3);
	}
	bool TryPrepareAIAction(CombatAction action) =>
		action switch
		{
			AttackBase attack => TryAssignRandomTargets(attack),
			TakeWeaponAction takeWeaponAction => takeWeaponAction.PrepareByAI(),
			PickWeaponAction pickWeaponAction => pickWeaponAction.PrepareByAI(),
			ReleaseAction { actorObject: BodyPart bodyPart, }
				when !bodyPart.HasBuff(BuffCode.Grappling, true) && bodyPart.Slots.Any(slot => slot.Item != null && (slot.Flag & ItemFlagCode.Arm) != 0)
				=> false,
			_ => true,
		};
	bool TryAssignRandomTargets(AttackBase attack)
	{
		var targets = attack.AvailableTargets.ToArray();
		if (targets.Length == 0) return false;
		var targetIndex = (int)(GD.Randi() % (uint)targets.Length);
		var target = targets[targetIndex];
		attack.target = target;
		var targetObjects = attack.AvailableTargetObjects.Where(t => !t.disabled).ToArray();
		if (targetObjects.Length == 0) return false;
		var objectIndex = (int)(GD.Randi() % (uint)targetObjects.Length);
		attack.targetObject = targetObjects[objectIndex].target;
		return true;
	}
}
