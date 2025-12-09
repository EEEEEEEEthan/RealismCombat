using System;
using Godot;
/// <summary>
///     反应成功率的计算结果
/// </summary>
public readonly struct ReactionChance
{
	public ReactionChance(double dodgeChance, double blockChance)
	{
		DodgeChance = dodgeChance;
		BlockChance = blockChance;
	}
	/// <summary>
	///     闪避成功率
	/// </summary>
	public double DodgeChance { get; }
	/// <summary>
	///     格挡成功率
	/// </summary>
	public double BlockChance { get; }
}
/// <summary>
///     反应结算结果
/// </summary>
public readonly struct ReactionOutcome
{
	public ReactionOutcome(ReactionTypeCode type, ICombatTarget? blockTarget, bool succeeded, double successChance)
	{
		Type = type;
		BlockTarget = blockTarget;
		Succeeded = succeeded;
		SuccessChance = successChance;
	}
	/// <summary>
	///     反应类型
	/// </summary>
	public ReactionTypeCode Type { get; }
	/// <summary>
	///     成功格挡时使用的目标
	/// </summary>
	public ICombatTarget? BlockTarget { get; }
	/// <summary>
	///     是否成功
	/// </summary>
	public bool Succeeded { get; }  // todo: 删掉。BlockTarget != null 就表示成功格挡了
	/// <summary>
	///     本次判定的成功率
	/// </summary>
	public double SuccessChance { get; }
}
/// <summary>
///     负责计算并结算闪避与格挡成功率
/// </summary>
public static class ReactionSuccessCalculator
{
	const double WeaponLengthScale = 50.0;
	const double WeaponWeightScale = 6.0;
	const double DefenderLoadScale = 80.0;
	const double BaseBodyWeight = 70.0;
	const double DodgeLengthWeight = 0.8;
	const double DodgeWeaponWeight = 0.9;
	const double DodgeActionWeight = 1.1;
	const double DodgeLoadWeight = 0.8;
	const double BlockLengthWeight = 0.9;
	const double BlockWeaponWeight = 0.9;
	const double BlockActionWeight = 1.2;
	const double BlockLoadWeight = 0.8;
	const double BlockCenterBonus = 0.35;
	const double UnarmedDodgeShift = 0.5;
	const double UnarmedBlockShift = 0.3;
	const double DodgeBias = -0.15;
	const double BlockBias = -0.05;
	/// <summary>
	///     计算指定攻击对应的闪避和格挡成功率
	/// </summary>
	public static ReactionChance Calculate(AttackBase attack)
	{
		var weapon = attack.UsesWeapon ? GetWeaponInUse(attack.ActorBodyPart) : null;
		var weaponLengthScore = weapon == null ? 0.0 : ScaleToRange(weapon.Length, WeaponLengthScale);
		var weaponWeightScore = weapon == null ? 0.0 : ScaleToRange(weapon.Weight, WeaponWeightScale);
		var defenderLoadScore = ScaleToRange(BaseBodyWeight + GetEquippedWeight(attack.target), DefenderLoadScale);
		var isUnarmedAttack = weapon == null || !attack.UsesWeapon;
		var dodgeScore = DodgeBias
		                 - DodgeLengthWeight * weaponLengthScore
		                 + DodgeWeaponWeight * weaponWeightScore
		                 + DodgeActionWeight * attack.DodgeImpact
		                 - DodgeLoadWeight * defenderLoadScore
		                 + (isUnarmedAttack ? UnarmedDodgeShift : 0.0);
		var blockTargetBonus = 0.0;
		if (attack.targetObject is BodyPart { id: BodyPartCode.Torso or BodyPartCode.Groin, })
			blockTargetBonus = BlockCenterBonus;
		var blockScore = BlockBias
		                 + BlockLengthWeight * weaponLengthScore
		                 + BlockWeaponWeight * weaponWeightScore
		                 + BlockActionWeight * attack.BlockImpact
		                 - BlockLoadWeight * defenderLoadScore
		                 + (isUnarmedAttack ? UnarmedBlockShift : 0.0)
		                 + blockTargetBonus;
		return new ReactionChance(
			Sigmoid(dodgeScore),
			Sigmoid(blockScore)
		);
	}
	/// <summary>
	///     根据玩家选择的反应计算是否成功
	/// </summary>
	public static ReactionOutcome Resolve(ReactionDecision decision, AttackBase attack)
	{
		var chance = Calculate(attack);
		var selectedChance = decision.type switch
		{
			ReactionTypeCode.Dodge => chance.DodgeChance,
			ReactionTypeCode.Block => chance.BlockChance,
			_ => 0.0,
		};
		var success = decision.type switch
		{
			ReactionTypeCode.Dodge => GD.Randf() < selectedChance,
			ReactionTypeCode.Block => GD.Randf() < selectedChance,
			_ => false,
		};
		return new ReactionOutcome(decision.type, decision.blockTarget, success, selectedChance);
	}
	static double GetEquippedWeight(Character character)
	{
		var total = 0.0;
		foreach (var bodyPart in character.bodyParts)
		{
			total += GetContainerWeight(bodyPart);
		}
		return total;
	}
	static double GetContainerWeight(IItemContainer container)
	{
		var total = 0.0;
		foreach (var slot in container.Slots)
		{
			if (slot.Item == null) continue;
			total += slot.Item.Weight;
			if (slot.Item.Slots.Length > 0) total += GetContainerWeight(slot.Item);
		}
		return total;
	}
	static Item? GetWeaponInUse(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
		{
			if (slot.Item != null && (slot.Item.flag & ItemFlagCode.Arm) != 0) return slot.Item;
		}
		return null;
	}
	static double ScaleToRange(double value, double scale) => 2.0 * Math.Tanh(value / scale);
	static double Sigmoid(double value) => 1.0 / (1.0 + Math.Exp(-value));
}

