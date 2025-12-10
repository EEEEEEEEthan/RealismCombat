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
				remaining = remaining.ApplyProtection(protection);
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
			if (targetObject is BodyPart { id: BodyPartCode.Torso or BodyPartCode.Groin, }) blockTargetBonus = blockCenterBonus;
			var blockScore =
				blockBias +
				blockLengthWeight * weaponLengthScore +
				blockWeaponWeight * weaponWeightScore +
				blockActionWeight * BlockImpact -
				blockLoadWeight * defenderLoadScore +
				(isUnarmedAttack ? unarmedBlockShift : 0.0) +
				blockTargetBonus;
			return new(
				GameMath.Sigmoid(dodgeScore),
				GameMath.Sigmoid(blockScore)
			);
		}
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
			ReactionTypeCode.Block => chance.BlockChance,
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
				await Task.Delay((int)(ResourceTable.blockSound.Value.GetLength() * 1000));
				await performHit(reactionOutcome.BlockTarget, dialogue);
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
		await performHit(targetObject, dialogue);
		await OnAttackLanded(target, targetObject, dialogue);
	END:
		actor.actionPoint.value = Math.Max(0, actor.actionPoint.value - postCastActionPointCost);
		actorNode.MoveTo(actorPosition);
		return;
		async Task performHit(ICombatTarget targetObject, GenericDialogue dialogue)
		{
			if (Damage.Total.RoundToInt() <= 0) return;
			if (targetObject is not IItemContainer targetContainer) throw new InvalidOperationException("目标不支持受击处理");
			var armors = targetContainer.IterItems(ItemFlagCode.Armor).ToList();
			if (armors.Count <= 0)
				switch (targetObject)
				{
					case BodyPart:
					{
						await dialogue.ShowTextTask($"{targetObject}没有任何防护，攻击硬生生打在了身上!");
						await applyDamage(this.target!, targetObject, Damage.Total.RoundToInt(), dialogue);
						return;
					}
					case Item item:
					{
						await applyDamage(this.target!, targetObject, (Damage - item.Protection).Total.RoundToInt(), dialogue);
						return;
					}
					default:
						throw new InvalidOperationException("未知的目标类型");
				}
			var hits = new bool[armors.Count];
			for (var i = 0; i < hits.Length; i++)
				if (GD.Randf() < armors[i].Coverage)
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
			var any = false;
			for (var i = firstHit; i >= 0; i--)
			{
				var item = armors[i];
				var damageToArmor = (damage.Slash - item.Protection.Slash).RoundToInt();
				any = any || await applyDamage(target, item, damageToArmor, dialogue);
				damage -= item.Protection;
				if (damage.Total <= 0) break;
			}
			any = any || await applyDamage(target, targetObject, damage.Total.RoundToInt(), dialogue);
			// 对武器的伤害
			if (UsesWeapon && actorBodyPart.WeaponInUse is { } weapon)
			{
				var weaponDamage = (damage - weapon.Protection).Total.RoundToInt();
				any = any || await applyDamage(actor, weapon, weaponDamage, dialogue);
			}
			if (!any) await dialogue.ShowTextTask("什么也没发生");
		}
		async Task<bool> applyDamage(Character character, ICombatTarget target, int damage, GenericDialogue dialogue)
		{
			var any = false;
			if (damage <= 0) return any;
			if (target is BodyPart)
			{
				await Task.Delay(100);
				var characterNode = combat.combatNode.GetCharacterNode(character);
				characterNode.Shake();
				AudioManager.PlaySfx(ResourceTable.retroHurt1);
				target.HitPoint.value -= damage;
				await dialogue.ShowTextTask($"{character.name}的{target.Name}受到{damage}点伤害");
				any = true;
			}
			else
			{
				var rate = (float)damage / target.HitPoint.maxValue;
				target.HitPoint.value -= damage;
				switch (rate)
				{
					case < 0.2f:
						return any;
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
				any = true;
			}
			return any;
		}
	}
}
