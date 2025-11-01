using System;
using Godot;
namespace RealismCombat.Data;
public class ReactionSimulate
{
	readonly CharacterData attacker;
	readonly CharacterData defender;
	readonly ActionData action;
	public ReactionSimulate(CharacterData attacker, CharacterData defender, ActionData action)
	{
		this.attacker = attacker;
		this.defender = defender;
		this.action = action;
	}
	public double CalculateDodgeSuccessRate()
	{
		var attackerBodyPart = action.attackerBody switch
		{
			BodyPartCode.Head => attacker.head,
			BodyPartCode.Chest => attacker.chest,
			BodyPartCode.LeftArm => attacker.leftArm,
			BodyPartCode.RightArm => attacker.rightArm,
			BodyPartCode.LeftLeg => attacker.leftLeg,
			BodyPartCode.RightLeg => attacker.rightLeg,
			_ => throw new ArgumentOutOfRangeException(),
		};
		double weaponWeight = 0.0;
		double weaponLength = 0.0;
		if (action.actionCode == ActionCode.Swing || action.actionCode == ActionCode.Thrust)
		{
			var weaponSlotIndex = attackerBodyPart.id switch
			{
				BodyPartCode.LeftArm => 1,
				BodyPartCode.RightArm => 1,
				_ => -1,
			};
			if (weaponSlotIndex >= 0 && weaponSlotIndex < attackerBodyPart.slots.Length)
			{
				var weaponSlot = attackerBodyPart.slots[weaponSlotIndex];
				if (weaponSlot.item != null)
				{
					weaponWeight = weaponSlot.item.weight;
					weaponLength = weaponSlot.item.length;
				}
			}
		}
		if (weaponWeight <= 0 || weaponLength <= 0 || defender.bodyWeight <= 0)
		{
			return 0.5;
		}
		var dodgeRate = weaponWeight / weaponLength / defender.bodyWeight;
		return Math.Clamp(dodgeRate, 0.0, 1.0);
	}
	public double CalculateBlockDamageReduction()
	{
		return 0.5;
	}
}

