using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
public class CharacterData
{
	public static readonly (double min, double max) InitialActionPointRange = (-10, 0);
	public readonly string name;
	public readonly byte team;
	public readonly BodyPartData head = new(BodyPartCode.Head);
	public readonly BodyPartData chest = new(BodyPartCode.Chest);
	public readonly BodyPartData leftArm = new(BodyPartCode.LeftArm);
	public readonly BodyPartData rightArm = new(BodyPartCode.RightArm);
	public readonly BodyPartData leftLeg = new(BodyPartCode.LeftLeg);
	public readonly BodyPartData rightLeg = new(BodyPartCode.RightLeg);
	public readonly IReadOnlyList<BodyPartData> bodyParts;
	public double speed = 1;
	public int reaction;
	double actionPoint;
	public double bodyWeight
	{
		get
		{
			var totalWeight = 70.0;
			foreach (var bodyPart in bodyParts)
			{
				foreach (var slot in bodyPart.slots)
					if (slot.item != null)
						totalWeight += slot.item.GetTotalWeight();
			}
			return totalWeight;
		}
	}
	public bool PlayerControlled => team == 0;
	public bool Dead => head.hp <= 0 || chest.hp <= 0;
	public double ActionPoint
	{
		get => actionPoint;
		set
		{
			actionPoint = value;
			actionPoint = actionPoint.Clamped(min: InitialActionPointRange.min, max: InitialActionPointRange.max);
		}
	}
	public CharacterData(string name, byte team)
	{
		this.name = name;
		this.team = team;
		bodyParts = new List<BodyPartData>
		{
			head, chest, leftArm, rightArm, leftLeg, rightLeg,
		};
	}
	public CharacterData(DataVersion version, BinaryReader reader)
	{
		name = reader.ReadString();
		team = reader.ReadByte();
		ActionPoint = reader.ReadDouble();
		speed = reader.ReadDouble();
		reaction = reader.ReadInt32();
		using (reader.ReadScope())
		{
			head = new(version: version, reader: reader);
			chest = new(version: version, reader: reader);
			leftArm = new(version: version, reader: reader);
			rightArm = new(version: version, reader: reader);
			leftLeg = new(version: version, reader: reader);
			rightLeg = new(version: version, reader: reader);
		}
		bodyParts = new List<BodyPartData>
		{
			head, chest, leftArm, rightArm, leftLeg, rightLeg,
		};
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(name);
		writer.Write(team);
		writer.Write(ActionPoint);
		writer.Write(speed);
		writer.Write(reaction);
		using (writer.WriteScope())
		{
			head.Serialize(writer);
			chest.Serialize(writer);
			leftArm.Serialize(writer);
			rightArm.Serialize(writer);
			leftLeg.Serialize(writer);
			rightLeg.Serialize(writer);
		}
	}
	public override string ToString() =>
		$"{nameof(CharacterData)}({nameof(name)}={name}, {nameof(team)}={team}, {nameof(ActionPoint)}={ActionPoint}, {nameof(speed)}={speed}, {nameof(bodyWeight)}={bodyWeight}, {nameof(reaction)}={reaction}, {nameof(head)}={head}, {nameof(chest)}={chest}, {nameof(leftArm)}={leftArm}, {nameof(rightArm)}={rightArm}, {nameof(leftLeg)}={leftLeg}, {nameof(rightLeg)}={rightLeg})";
}
public enum BodyPartCode
{
	Head,
	Chest,
	LeftArm,
	RightArm,
	LeftLeg,
	RightLeg,
}
public static class BodyPartCodeExtensions
{
	public static string GetName(this BodyPartCode part) =>
		part switch
		{
			BodyPartCode.Head => "头部",
			BodyPartCode.Chest => "躯干",
			BodyPartCode.LeftArm => "左臂",
			BodyPartCode.RightArm => "右臂",
			BodyPartCode.LeftLeg => "左腿",
			BodyPartCode.RightLeg => "右腿",
			_ => "未知部位",
		};
	public static int GetSlotCapacity(this BodyPartCode part) =>
		part switch
		{
			BodyPartCode.LeftArm => 2,
			BodyPartCode.RightArm => 2,
			_ => 1,
		};
	public static EquipmentTypeCode GetAllowedTypes(this BodyPartCode part, int slotIndex) =>
		part switch
		{
			BodyPartCode.Head => EquipmentTypeCode.HelmetLiner,
			BodyPartCode.Chest => EquipmentTypeCode.ChestLiner,
			BodyPartCode.LeftArm => slotIndex == 0 ? EquipmentTypeCode.Gauntlet : EquipmentTypeCode.Arm,
			BodyPartCode.RightArm => slotIndex == 0 ? EquipmentTypeCode.Gauntlet : EquipmentTypeCode.Arm,
			BodyPartCode.LeftLeg => EquipmentTypeCode.LegLiner,
			BodyPartCode.RightLeg => EquipmentTypeCode.LegLiner,
			_ => EquipmentTypeCode.None,
		};
}
public class BodyPartData : IItemContainer
{
	public static readonly IReadOnlyList<BodyPartCode> allBodyParts = Enum.GetValues<BodyPartCode>();
	public readonly BodyPartCode id;
	public readonly SlotData[] slots;
	public int hp = 10;
	public int maxHp = 10;
	public IReadOnlyList<ItemData?> items => slots.Select(s => s.item).ToList().AsReadOnly();
	public event Action? ItemsChanged;
	public BodyPartData(BodyPartCode id)
	{
		this.id = id;
		var capacity = id.GetSlotCapacity();
		slots = new SlotData[capacity];
		for (var i = 0; i < capacity; i++)
		{
			var allowedTypes = id.GetAllowedTypes(slotIndex: i);
			slots[i] = new(allowedTypes: allowedTypes);
		}
	}
	public BodyPartData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			id = (BodyPartCode)reader.ReadByte();
			hp = reader.ReadInt32();
			maxHp = reader.ReadInt32();
			var slotCount = reader.ReadInt32();
			var capacity = id.GetSlotCapacity();
			slots = new SlotData[capacity];
			for (var i = 0; i < slotCount && i < capacity; ++i) slots[i] = new(version: version, reader: reader);
			for (var i = slotCount; i < capacity; i++)
			{
				var allowedTypes = id.GetAllowedTypes(slotIndex: i);
				slots[i] = new(allowedTypes: allowedTypes);
			}
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write((byte)id);
			writer.Write(hp);
			writer.Write(maxHp);
			writer.Write(slots.Length);
			foreach (var slot in slots) slot.Serialize(writer);
		}
	}
	public void SetSlot(int index, ItemData? value)
	{
		if (index < 0 || index >= slots.Length) throw new ArgumentOutOfRangeException(nameof(index));
		if (!slots[index].CanPlace(value)) throw new InvalidOperationException($"物品类型 {value?.itemId} 不允许放入此槽位");
		slots[index].item = value;
		ItemsChanged?.Invoke();
	}
	public override string ToString() =>
		$"{nameof(BodyPartData)}({nameof(id)}={id}, {nameof(hp)}={hp}, {nameof(maxHp)}={maxHp}, {nameof(slots)}={slots.Length})";
}
