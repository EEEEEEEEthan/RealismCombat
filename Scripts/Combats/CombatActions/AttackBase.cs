using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     攻击基类
/// </summary>
public abstract class AttackBase(Character actor, BodyPart actorBodyPart, Combat combat, double preCastActionPointCost, double postCastActionPointCost)
	: CombatAction(actor, combat, actorBodyPart, preCastActionPointCost, postCastActionPointCost)
{
	/// <summary>
	///     计算在当前防护判定规则下本体受到的期望伤害
	/// </summary>
	/// <param name="damage">攻击伤害</param>
	/// <param name="targetObject">受击目标</param>
	/// <returns>期望的本体伤害</returns>
	public static double CalculateExpectedBodyDamage(Damage damage, ICombatTarget targetObject)
	{
		if (damage.Total <= 0f) return 0d;
		if (targetObject is not IItemContainer container) return damage.Total.RoundToInt();
		var armors = container.IterItems(ItemFlagCode.Armor).ToList();
		if (armors.Count == 0) return damage.Total.RoundToInt();
		Damage ComputeResidual(int firstHit)
		{
			var remaining = damage;
			for (var i = firstHit; i >= 0 && remaining.Total > 0f; i--)
			{
				var protection = armors[i].Protection;
				remaining -= protection;
			}
			return remaining;
		}
		var expected = 0d;
		var firstCoverage = armors[0].Coverage;
		var noArmorProb = 1d - firstCoverage;
		expected += noArmorProb * damage.Total.RoundToInt();
		var prefix = firstCoverage;
		for (var i = 0; i < armors.Count; i++)
		{
			double scenarioProb;
			if (i == armors.Count - 1)
			{
				scenarioProb = prefix;
			}
			else
			{
				var nextCoverage = armors[i + 1].Coverage;
				scenarioProb = prefix * (1d - nextCoverage);
				prefix *= nextCoverage;
			}
			var residual = ComputeResidual(i);
			expected += scenarioProb * residual.Total.RoundToInt();
		}
		return expected;
	}
	public readonly BodyPart actorBodyPart = actorBodyPart;
	public Character? target;
	public ICombatTarget? targetObject;
	public override string Description
	{
		get
		{
			var builder = new StringBuilder();
			builder.AppendLine($"类型:{AttackType switch
			{
				AttackTypeCode.Swing => "挥砍",
				AttackTypeCode.Thrust => "刺击",
				AttackTypeCode.Special => "特殊",
				_ => "未知",
			}}");
			builder.AppendLine(DodgeImpact switch
			{
				>= 0.6 => "容易被闪避",
				>= 0.4 => "中等闪避难度",
				_ => "不易被闪避",
			});
			builder.AppendLine(BlockImpact switch
			{
				>= 0.6 => "容易被格挡",
				>= 0.4 => "中等格挡难度",
				_ => "不易被格挡",
			});
			builder.Append(Narrative);
			return builder.ToString();
		}
	}
	public override bool Visible => actorBodyPart.Available && IsBodyPartUsable(actorBodyPart);
	public abstract CombatActionCode Id { get; }
	public virtual IEnumerable<Character> AvailableTargets => Opponents.Where(c => c.IsAlive);
	public virtual IEnumerable<(ICombatTarget target, bool disabled)> AvailableTargetObjects
	{
		get
		{
			var target = this.target;
			if (target == null) return [];
			return target.AvailableCombatTargets.Select(t => (t, !t.Available));
		}
	}
	public abstract string Narrative { get; }
	public BodyPart ActorBodyPart => actorBodyPart;
	public virtual Damage Damage
	{
		get
		{
			var attackType = AttackType;
			if (UsesWeapon && attackType != AttackTypeCode.Special)
				foreach (var slot in ActorBodyPart.Slots)
				{
					var weapon = slot.Item;
					if (weapon != null && (weapon.flag & ItemFlagCode.Arm) != 0) return weapon.DamageProfile.Get(attackType);
				}
			return Damage.Zero;
		}
	}
	public abstract double DodgeImpact { get; }
	public abstract double BlockImpact { get; }
	public abstract AttackTypeCode AttackType { get; }
	public virtual bool UsesWeapon => false;
	/// <summary>
	///     盔甲缝隙系数，Coverage会被power处理。默认1.0，半剑式为2.0
	/// </summary>
	public virtual double ArmorGapPower => 1.0;
	public abstract string? PreCastText { get; }
	public abstract string CastText { get; }
	/// <summary>
	///     计算当前攻击对应的闪避与格挡成功率
	/// </summary>
	public ReactionChance ReactionChance
	{
		get
		{
			const double weaponLengthScale = 50.0;
			const double weaponWeightScale = 6.0;
			const double defenderLoadScale = 80.0;
			const double baseBodyWeight = 70.0;
			const double dodgeLengthWeight = 0.8;
			const double dodgeWeaponWeight = 0.9;
			const double dodgeActionWeight = 1.1;
			const double dodgeLoadWeight = 0.8;
			const double blockLengthWeight = 0.9;
			const double blockWeaponWeight = 0.9;
			const double blockActionWeight = 1.2;
			const double blockLoadWeight = 0.8;
			const double blockCenterBonus = 0.35;
			const double unarmedDodgeShift = 0.5;
			const double unarmedBlockShift = 0.3;
			const double dodgeBias = -0.05;
			const double blockBias = -0.05;
			var weapon = UsesWeapon ? ActorBodyPart.WeaponInUse : null;
			var weaponLengthScore = weapon == null ? 0.0 : GameMath.ScaleToRange(weapon.Length, weaponLengthScale);
			var weaponWeightScore = weapon == null ? 0.0 : GameMath.ScaleToRange(weapon.Weight, weaponWeightScale);
			var defenderLoadScore = GameMath.ScaleToRange(baseBodyWeight + target!.EquippedWeight, defenderLoadScale);
			var isUnarmedAttack = weapon == null || !UsesWeapon;
			var dodgeScore =
				dodgeBias -
				dodgeLengthWeight * weaponLengthScore +
				dodgeWeaponWeight * weaponWeightScore +
				dodgeActionWeight * DodgeImpact -
				dodgeLoadWeight * defenderLoadScore +
				(isUnarmedAttack ? unarmedDodgeShift : 0.0);
			var blockTargetBonus = 0.0;
			if (targetObject is BodyPart { id: BodyPartCode.Torso, }) blockTargetBonus = blockCenterBonus;
			var blockScore =
				blockBias +
				blockLengthWeight * weaponLengthScore +
				blockWeaponWeight * weaponWeightScore +
				blockActionWeight * BlockImpact -
				blockLoadWeight * defenderLoadScore +
				(isUnarmedAttack ? unarmedBlockShift : 0.0) +
				blockTargetBonus;
			var dodgeChance = GameMath.Sigmoid(dodgeScore);
			var blockChance = GameMath.Sigmoid(blockScore);
			if (target is { } defender)
			{
				// 若角色任意部位有倒伏buff，则闪避成功率为0
				var hasProneBuff = defender.bodyParts.Any(bp => bp.HasBuff(BuffCode.Prone, false));
				if (hasProneBuff) dodgeChance = 0.0;
				var legRestrained = defender.bodyParts.Any(bp => bp.id.IsLeg && !bp.Free);
				if (legRestrained) dodgeChance /= 5.0;
				var anyPartNotFree = defender.bodyParts.Any(bp => !bp.Free);
				if (anyPartNotFree)
				{
					// 若角色任意部位被束缚/非 Free，则闪避与格挡成功率下降到原来的1/3
					dodgeChance /= 3.0;
					blockChance /= 3.0;
				}
			}
			return new(
				dodgeChance,
				blockChance
			);
		}
	}
	/// <summary>
	///     在角色的身体部位中查找物品所属的部位
	/// </summary>
	static BodyPart? FindItemOwner(Item item, Character character)
	{
		foreach (var bodyPart in character.bodyParts)
		{
			foreach (var slot in bodyPart.Slots)
			{
				if (ReferenceEquals(slot.Item, item))
					return bodyPart;
				// 递归检查嵌套物品
				if (slot.Item != null && ContainsItemRecursive(slot.Item, item))
					return bodyPart;
			}
		}
		return null;
	}
	/// <summary>
	///     递归检查容器是否包含指定物品
	/// </summary>
	static bool ContainsItemRecursive(IItemContainer container, Item target)
	{
		foreach (var slot in container.Slots)
		{
			if (ReferenceEquals(slot.Item, target))
				return true;
			if (slot.Item != null && ContainsItemRecursive(slot.Item, target))
				return true;
		}
		return false;
	}
	/// <summary>
	///     计算使用特定部位格挡时的成功率修正系数
	/// </summary>
	/// <param name="blockTarget">用于格挡的目标（身体部位或武器）</param>
	/// <returns>格挡成功率的乘数，范围从0.05（几乎不可能）到1.0（无惩罚）</returns>
	public double CalculateBlockChanceModifier(ICombatTarget blockTarget)
	{
		if (targetObject is not BodyPart attackedPart) return 1.0; // 攻击目标不是身体部位时无惩罚
		
		// 获取格挡部位
		BodyPart? blockingPart = blockTarget switch
		{
			BodyPart bp => bp,
			Item item => FindItemOwner(item, target!), // 武器所在的身体部位
			_ => null,
		};
		
		if (blockingPart == null) return 1.0;
		
		// 使用武器格挡时有额外加成
		var isUsingWeapon = blockTarget is Item;
		var weaponBonus = isUsingWeapon ? 0.15 : 0.0;
		
		// 计算高度差
		var attackHeight = attackedPart.id.NormalizedHeight;
		var blockHeight = blockingPart.id.NormalizedHeight;
		var heightDiff = System.Math.Abs(attackHeight - blockHeight);
		
		// 特殊规则：用头格挡任何部位都很困难
		if (blockingPart.id == BodyPartCode.Head)
		{
			// 用头格挡头部攻击稍微好一点，但仍然很困难
			return attackedPart.id == BodyPartCode.Head ? 0.3 : 0.05;
		}
		
		// 特殊规则：用腿格挡上半身攻击很困难
		if (blockingPart.id.IsLeg && attackedPart.id == BodyPartCode.Head)
		{
			return 0.08;
		}
		
		// 基于高度差的惩罚
		// 高度差越大，格挡越困难
		// 0.0 差距 -> 无惩罚
		// 0.5 差距 -> 约50%成功率
		// 1.0 差距 -> 约15%成功率
		var heightPenalty = System.Math.Exp(-heightDiff * 3.0);
		
		// 同类型部位格挡有加成
		var sameTypeBonus = 0.0;
		if (blockingPart.id.IsArm && attackedPart.id.IsArm) sameTypeBonus = 0.1;
		else if (blockingPart.id.IsLeg && attackedPart.id.IsLeg) sameTypeBonus = 0.1;
		
		// 用躯干格挡有额外加成（身体最大部位）
		var torsoBonus = blockingPart.id == BodyPartCode.Torso ? 0.1 : 0.0;
		
		// 综合计算
		var modifier = heightPenalty + weaponBonus + sameTypeBonus + torsoBonus;
		
		// 限制在合理范围内
		return System.Math.Clamp(modifier, 0.05, 1.0);
	}
	IEnumerable<Character> Opponents => combat.Allies.Contains(actor) ? combat.Enemies : combat.Allies;
	protected abstract bool IsBodyPartUsable(BodyPart bodyPart);
	protected virtual Task OnAttackLanded(Character targetCharacter, ICombatTarget targetObject, GenericDialogue dialogue) => Task.CompletedTask;
	protected override async Task OnStartTask()
	{
		var text = PreCastText;
		if (string.IsNullOrEmpty(text)) return;
		await DialogueManager.ShowGenericDialogue(text);
	}
	protected override async Task OnExecute()
	{
		var target = this.target!;
		var targetObject = this.targetObject!;
		var actorWeapon = UsesWeapon ? actorBodyPart.WeaponInUse : null;
		var actorNode = combat.combatNode.GetCharacterNode(actor);
		var targetNode = combat.combatNode.GetCharacterNode(target);
		var actorPosition = combat.combatNode.GetPKPosition(actor);
		var targetPosition = combat.combatNode.GetPKPosition(target);
		using var _ = actorNode.MoveScope(actorPosition);
		using var __ = targetNode.MoveScope(targetPosition);
		using var ___ = actorNode.ExpandScope();
		using var ____ = targetNode.ExpandScope();
		await DialogueManager.ShowGenericDialogue(CastText);
		var reaction = await combat.HandleIncomingAttack(this);
		var chance = ReactionChance;
		var selectedChance = reaction.type switch
		{
			ReactionTypeCode.Dodge => chance.DodgeChance,
			ReactionTypeCode.Block => chance.BlockChance * CalculateBlockChanceModifier(reaction.blockTarget!),
			_ => 0.0,
		};
		var success = reaction.type switch
		{
			ReactionTypeCode.Dodge or ReactionTypeCode.Block => GD.Randf() < selectedChance,
			_ => false,
		};
		var blockTarget = reaction.type switch
		{
			ReactionTypeCode.Block => reaction.blockTarget ?? throw new InvalidOperationException("格挡结果缺少目标"),
			_ => targetObject,
		};
		var reactionOutcome = new ReactionOutcome(reaction.type, blockTarget, success, selectedChance);
		var hitPosition = combat.combatNode.GetHitPosition(actor);
		actorNode.MoveTo(hitPosition);
		using var _____ = DialogueManager.CreateGenericDialogue(out var dialogue);
		switch (reactionOutcome.Type)
		{
			case ReactionTypeCode.Dodge when reactionOutcome.Succeeded:
			{
				await Task.Delay(10);
				targetNode.MoveTo(combat.combatNode.GetDogePosition(target));
				await dialogue.ShowTextTask($"{target.name}闪避成功");
				goto END;
			}
			case ReactionTypeCode.Dodge:
			{
				await dialogue.ShowTextTask($"{target.name}尝试闪避但失败");
				await Task.Delay(100);
				goto FALLBACK;
			}
			case ReactionTypeCode.Block when reactionOutcome is { Succeeded: true, }:
			{
				await Task.Delay(50);
				targetNode.MoveTo(targetPosition + Vector2.Up * 12);
				targetNode.FlashFrame();
				await Task.Delay(100);
				targetNode.MoveTo(targetPosition);
				AudioManager.PlaySfx(ResourceTable.blockSound, 6f);
				await dialogue.ShowTextTask($"{target.name}使用{reactionOutcome.BlockTarget.Name}挡住了攻击");
				// 如果防御者正在执行招架动作，格挡成功时+1反应
				if (target.combatAction is ParryAction)
				{
					target.reaction += 1;
					await dialogue.ShowTextTask($"{target.name}反应+1");
				}
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
				await performHit(reactionOutcome.BlockTarget, dialogue);
				// 如果用武器格挡，攻击方的部位需要承受武器基础伤害的一半
				if (reactionOutcome.BlockTarget is Item item && (item.flag & ItemFlagCode.Arm) != 0)
				{
					var damage = item.DamageProfile.Swing * 0.5f;
					if (actorWeapon != null)
					{
						await applyDamage(actor, actorWeapon, damage - actorWeapon.Protection, dialogue);
					}
					else
					{
						var protection = Protection.Zero;
						foreach (var armor in actorBodyPart.IterItems(ItemFlagCode.Armor)) protection += armor.Protection;
						await applyDamage(actor, actorBodyPart, damage - protection, dialogue);
					}
				}
				goto END;
			}
			case ReactionTypeCode.Block:
			{
				await dialogue.ShowTextTask($"{target.name}尝试格挡但失败");
				await Task.Delay(100);
				goto FALLBACK;
			}
			case ReactionTypeCode.None:
			{
				await Task.Delay(100);
				goto FALLBACK;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}
	FALLBACK:
		var hitPointBeforeDamage = targetObject.HitPoint.value;
		var bodyDamage = await performHit(targetObject, dialogue);
		// 格挡失败或承受时，有概率打断行动，概率为伤害值/伤害前的部位hp
		if (bodyDamage > 0 && targetObject is BodyPart && target.combatAction != null && hitPointBeforeDamage > 0)
		{
			var interruptChance = bodyDamage / hitPointBeforeDamage;
			if (GD.Randf() < interruptChance)
			{
				target.combatAction = null;
				await dialogue.ShowTextTask($"{target.name}的行动被打断!");
			}
		}
		await OnAttackLanded(target, targetObject, dialogue);
	END:
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - postCastActionPointCost);
		actorNode.MoveTo(actorPosition);
		return;
		async Task<float> performHit(ICombatTarget targetObject, GenericDialogue dialogue)
		{
			if (Damage.Total.RoundToInt() <= 0) return 0f;
			if (targetObject is not IItemContainer targetContainer) throw new InvalidOperationException("目标不支持受击处理");
			var armors = targetContainer.IterItems(ItemFlagCode.Armor).ToList();
			if (armors.Count <= 0)
				switch (targetObject)
				{
					case BodyPart:
					{
						await dialogue.ShowTextTask($"{targetObject.Name}没有任何防护，攻击硬生生打在了身上!");
						return await applyDamage(this.target!, targetObject, Damage, dialogue);
					}
					case Item item:
					{
						// 对目标物体的伤害
						await applyDamage(this.target!, targetObject, Damage - item.Protection, dialogue);
						// 对武器的伤害
						if (actorWeapon != null)
						{
							await applyDamage(actor, actorWeapon, Damage - actorWeapon.Protection, dialogue);
						}
						return 0f;
					}
					default:
						throw new InvalidOperationException("未知的目标类型");
				}
			var hits = new bool[armors.Count];
			for (var i = 0; i < hits.Length; i++)
				if (GD.Randf() < Math.Pow(armors[i].Coverage, ArmorGapPower))
					hits[i] = true;
				else
					break;
			var missed = new List<string>();
			var firstHit = -1;
			for (var i = armors.Count; i-- > 0;)
				if (!hits[i])
				{
					missed.Add($"{armors[i].Name}");
				}
				else
				{
					firstHit = i;
					break;
				}
			var textBuilder = new List<string>();
			if (missed.Count > 0) textBuilder.Add($"攻击避开了{string.Join("、", missed)}的防护");
			switch (firstHit)
			{
				case >= 0:
					textBuilder.Add($"打在了{armors[firstHit].Name}上");
					break;
				default:
					textBuilder.Add($"以完美的角度打在了{targetObject.Name}上");
					break;
			}
			await dialogue.ShowTextTask(string.Join("；", textBuilder));
			// 计算伤害
			var damage = Damage;
			var totalBodyDamage = 0f;
			var any = false;
			for (var i = firstHit; i >= 0; i--)
			{
				var item = armors[i];
				var slashToArmor = Math.Max(0f, damage.Slash - item.Protection.Slash);
				var armorDamage = await applyDamage(target, item, new(slashToArmor, 0f, 0f), dialogue);
				any = any || armorDamage > 0;
				damage -= item.Protection;
				if (damage.Total <= 0) break;
			}
			var finalDamage = await applyDamage(target, targetObject, damage, dialogue);
			any = any || finalDamage > 0;
			if (targetObject is BodyPart)
			{
				totalBodyDamage = finalDamage;
			}
			// 对武器的伤害
			if (actorWeapon != null)
			{
				var weaponDamage = await applyDamage(actor, actorWeapon, Damage - actorWeapon.Protection, dialogue);
				any = any || weaponDamage > 0;
			}
			if (!any) await dialogue.ShowTextTask("没有造成伤害");
			return totalBodyDamage;
		}
		async Task<float> applyDamage(Character character, ICombatTarget target, Damage damage, GenericDialogue dialogue)
		{
			var damageAmount = damage.Total.RoundToInt();
			if (damageAmount <= 0) return 0f;
			if (target is BodyPart bodyPart)
			{
				await Task.Delay(100);
				var characterNode = combat.combatNode.GetCharacterNode(character);
				characterNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				target.HitPoint.value -= damageAmount;
				var shouldBleed =
					(damage.Slash > 0f) ||
					(damage.Pierce > 0f && GD.Randf() < 0.5f);
				await dialogue.ShowTextTask($"{character.name}的{target.Name}受到{damageAmount}点伤害");
				if (shouldBleed && !bodyPart.HasBuff(BuffCode.Bleeding, false))
				{
					var source = new BuffSource(actor, actorBodyPart);
					bodyPart.Buffs[BuffCode.Bleeding] = new(BuffCode.Bleeding, source);
					await dialogue.ShowTextTask($"{character.name}的{target.Name}开始流血!");
				}
				return damageAmount;
			}
			else
			{
				var rate = damageAmount / (float)target.HitPoint.maxValue;
				target.HitPoint.value -= damageAmount;
				switch (rate)
				{
					case < 0.2f:
						break;
					case < 0.3f:
						await dialogue.ShowTextTask($"{character.name}的{target.Name}受到了一些损伤");
						break;
					default:
						await dialogue.ShowTextTask($"{character.name}的{target.Name}受到了严重损伤");
						break;
				}
			}
			if (target.HitPoint.value < 0)
			{
				await dialogue.ShowTextTask($"{target.Name}被击毁了!");
			}
			return damageAmount;
		}
	}
}
