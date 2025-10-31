using System;
using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public record ActionData
{
	public readonly int attackerIndex;
	public readonly int defenderIndex;
	public readonly BodyPartCode attackerBody;
	public readonly BodyPartCode defenderBody;
	public readonly ActionCode actionCode;
	public ActionData(int attackerIndex, BodyPartCode attackerBody, int defenderIndex, BodyPartCode defenderBody, ActionCode actionCode)
	{
		this.attackerIndex = attackerIndex;
		this.defenderIndex = defenderIndex;
		this.attackerBody = attackerBody;
		this.defenderBody = defenderBody;
		this.actionCode = actionCode;
	}
	public ActionData(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			attackerIndex = reader.ReadInt32();
			defenderIndex = reader.ReadInt32();
			attackerBody = (BodyPartCode)reader.ReadByte();
			defenderBody = (BodyPartCode)reader.ReadByte();
			actionCode = (ActionCode)reader.ReadByte();
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(attackerIndex);
			writer.Write(defenderIndex);
			writer.Write((byte)attackerBody);
			writer.Write((byte)defenderBody);
			writer.Write((byte)actionCode);
		}
	}
}
public enum ActionCode
{
	StraightPunch,
	HookPunch,
	Swing,
	Thrust,
	Kick,
	ElbowStrike,
	Headbutt,
	Charge,
}
public static class ActionCodeExtensions
{
	public static string GetName(this ActionCode motion) =>
		motion switch
		{
			ActionCode.StraightPunch => "冲拳",
			ActionCode.HookPunch => "勾拳",
			ActionCode.Swing => "挥砍",
			ActionCode.Thrust => "刺击",
			ActionCode.Kick => "踢",
			ActionCode.ElbowStrike => "肘击",
			ActionCode.Headbutt => "头槌",
			ActionCode.Charge => "冲撞",
			_ => "未知动作",
		};
}
public class ActionConfig
{
	public static readonly Dictionary<ActionCode, ActionConfig> Configs = new();
	static ActionConfig()
	{
		Configs[ActionCode.StraightPunch] = new(actionCode: ActionCode.StraightPunch, damageRange: (1, 3), actionPointCost: 5, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.HookPunch] = new(actionCode: ActionCode.HookPunch, damageRange: (2, 4), actionPointCost: 6, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Swing] = new(actionCode: ActionCode.Swing, damageRange: (3, 5), actionPointCost: 7, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Thrust] = new(actionCode: ActionCode.Thrust, damageRange: (2, 6), actionPointCost: 6, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Kick] = new(actionCode: ActionCode.Kick, damageRange: (4, 8), actionPointCost: 8, allowedBodyParts: new[] { BodyPartCode.LeftLeg, BodyPartCode.RightLeg });
		Configs[ActionCode.ElbowStrike] = new(actionCode: ActionCode.ElbowStrike, damageRange: (2, 4), actionPointCost: 4, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Headbutt] = new(actionCode: ActionCode.Headbutt, damageRange: (3, 6), actionPointCost: 6, allowedBodyParts: new[] { BodyPartCode.Head });
		Configs[ActionCode.Charge] = new(actionCode: ActionCode.Charge, damageRange: (5, 10), actionPointCost: 10, allowedBodyParts: new[] { BodyPartCode.Chest });
	}
	public (int min, int max) damageRange { get; private set; }
	public int actionPointCost { get; private set; }
	public ActionCode actionCode { get; private set; }
	public IReadOnlyList<BodyPartCode> allowedBodyParts { get; private set; }
	ActionConfig(ActionCode actionCode, (int min, int max) damageRange, int actionPointCost, IReadOnlyList<BodyPartCode> allowedBodyParts)
	{
		this.actionCode = actionCode;
		this.damageRange = damageRange;
		this.actionPointCost = actionPointCost;
		this.allowedBodyParts = allowedBodyParts;
	}
	public bool ValidEquipment(CharacterData attacker, BodyPartCode bodyPart, out string error)
	{
		var bodyPartData = bodyPart switch
		{
			BodyPartCode.Head => attacker.head,
			BodyPartCode.Chest => attacker.chest,
			BodyPartCode.LeftArm => attacker.leftArm,
			BodyPartCode.RightArm => attacker.rightArm,
			BodyPartCode.LeftLeg => attacker.leftLeg,
			BodyPartCode.RightLeg => attacker.rightLeg,
			_ => throw new ArgumentOutOfRangeException(),
		};
		if (actionCode == ActionCode.Swing || actionCode == ActionCode.Thrust)
		{
			var weaponSlotIndex = bodyPartData.id switch
			{
				BodyPartCode.LeftArm => 1,
				BodyPartCode.RightArm => 1,
				_ => -1,
			};
			if (weaponSlotIndex < 0 || weaponSlotIndex >= bodyPartData.slots.Length)
			{
				error = $"{bodyPart.GetName()}无法装备武器";
				return false;
			}
			var weaponSlot = bodyPartData.slots[weaponSlotIndex];
			if (weaponSlot.item == null)
			{
				error = $"{bodyPart.GetName()}未装备武器";
				return false;
			}
			var itemType = ItemConfig.Configs.TryGetValue(key: weaponSlot.item.itemId, value: out var itemConfig) ? itemConfig.equipmentType : EquipmentType.None;
			var hasWeapon = (itemType & (EquipmentType.OneHandedWeapon | EquipmentType.TwoHandedWeapon)) != 0;
			if (!hasWeapon)
			{
				error = $"{bodyPart.GetName()}装备的不是武器";
				return false;
			}
		}
		error = null!;
		return true;
	}
}
