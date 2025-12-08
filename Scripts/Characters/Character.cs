using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public class Character
{
	static readonly GameVersion combatActionSerializationVersion = new(0, 0, 1);
	static readonly GameVersion combatActionDictionarySerializationVersion = new(0, 0, 2);
	public readonly PropertyInt speed;
	public readonly PropertyDouble actionPoint;
	public readonly string name;
	public readonly Inventory inventory;
	public readonly BodyPart head;
	public readonly BodyPart leftArm;
	public readonly BodyPart rightArm;
	public readonly BodyPart torso;
	public readonly BodyPart groin;
	public readonly BodyPart leftLeg;
	public readonly BodyPart rightLeg;
	public readonly IReadOnlyList<BodyPart> bodyParts;
	public readonly Dictionary<CombatActionCode, float> availableCombatActions = new();
	public int reaction;
	public CombatAction? combatAction;
	public bool IsAlive => head.Available && torso.Available;
	public Character(string name)
	{
		this.name = name;
		speed = new(5, 5);
		actionPoint = new(0f, 10f);
		reaction = 1;
		inventory = new();
		bodyParts =
		[
			head = new(BodyPartCode.Head),
			leftArm = new(BodyPartCode.LeftArm),
			rightArm = new(BodyPartCode.RightArm),
			torso = new(BodyPartCode.Torso),
			groin = new(BodyPartCode.Groin),
			leftLeg = new(BodyPartCode.LeftLeg),
			rightLeg = new(BodyPartCode.RightLeg),
		];
		availableCombatActions[CombatActionCode.Slash] = 0f;
		availableCombatActions[CombatActionCode.Grab] = 0f;
		availableCombatActions[CombatActionCode.PickWeapon] = 0f;
	}
	public Character(BinaryReader reader) : this(reader, GameVersion.newest) { }
	public Character(BinaryReader reader, GameVersion version)
	{
		using (reader.ReadScope())
		{
			name = reader.ReadString();
			speed = new(reader);
			actionPoint = new(reader);
			inventory = new();
			bodyParts =
			[
				head = new(BodyPartCode.Head),
				leftArm = new(BodyPartCode.LeftArm),
				rightArm = new(BodyPartCode.RightArm),
				torso = new(BodyPartCode.Torso),
				groin = new(BodyPartCode.Groin),
				leftLeg = new(BodyPartCode.LeftLeg),
				rightLeg = new(BodyPartCode.RightLeg),
			];
			foreach (var bodyPart in bodyParts) bodyPart.Deserialize(reader);
			inventory.Deserialize(reader);
			reaction = 1;
			availableCombatActions.Clear();
			var pairCount = reader.ReadInt32();
			for (var i = 0; i < pairCount; i++)
			{
				var code = (CombatActionCode)reader.ReadInt32();
				var value = reader.ReadSingle();
				availableCombatActions[code] = value;
			}
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			if (availableCombatActions.Count == 0) availableCombatActions[CombatActionCode.Slash] = 0f;
			writer.Write(name);
			speed.Serialize(writer);
			actionPoint.Serialize(writer);
			foreach (var bodyPart in bodyParts) bodyPart.Serialize(writer);
			inventory.Serialize(writer);
			var pairs = availableCombatActions.ToArray();
			Array.Sort(pairs, (a, b) => ((int)a.Key).CompareTo((int)b.Key));
			writer.Write(pairs.Length);
			foreach (var pair in pairs)
			{
				writer.Write((int)pair.Key);
				writer.Write(pair.Value);
			}
		}
	}
	/// <summary>
	///     收集角色所有腰带上的武器槽
	/// </summary>
	public List<(Item Belt, ItemSlot Slot)> GetBeltWeaponCandidates()
	{
		var result = new List<(Item, ItemSlot)>();
		foreach (var bodyPart in bodyParts) CollectBeltWeapons(bodyPart, result);
		return result;
	}
	/// <summary>
	///     获取角色可用的空手部槽位
	/// </summary>
	public (BodyPart bodyPart, ItemSlot slot)? FindEmptyHandSlot()
	{
		foreach (var bodyPart in bodyParts)
		{
			if (!bodyPart.Available) continue;
			if (bodyPart.id is not (BodyPartCode.LeftArm or BodyPartCode.RightArm)) continue;
			foreach (var slot in bodyPart.Slots)
				if (slot.Item == null && (slot.Flag & ItemFlagCode.Arm) != 0)
					return (bodyPart, slot);
		}
		return null;
	}
	void CollectBeltWeapons(IItemContainer container, List<(Item, ItemSlot)> result)
	{
		foreach (var slot in container.Slots)
		{
			if (slot.Item == null) continue;
			var item = slot.Item;
			if ((item.flag & ItemFlagCode.Belt) != 0)
				foreach (var beltSlot in item.Slots)
					if (beltSlot.Item is { flag: var flag, } && (flag & ItemFlagCode.Arm) != 0)
						result.Add((item, beltSlot));
			CollectBeltWeapons(item, result);
		}
	}
}
