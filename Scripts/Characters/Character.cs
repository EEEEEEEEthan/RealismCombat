using System.Collections.Generic;
using System.IO;
using RealismCombat.Combats;
using RealismCombat.Combats.CombatActions;
using RealismCombat.Extensions;
using RealismCombat.Items;
namespace RealismCombat.Characters;
public class Character
{
	public readonly PropertyInt speed;
	public readonly PropertyDouble actionPoint;
	public readonly string name;
	public readonly Inventory inventory;
	public readonly BodyPart head;
	public readonly BodyPart leftArm;
	public readonly BodyPart rightArm;
	public readonly BodyPart torso;
	public readonly BodyPart leftLeg;
	public readonly BodyPart rightLeg;
	public readonly IReadOnlyList<BodyPart> bodyParts;
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
			head = new(BodyPartCode.Head, []),
			leftArm = new(BodyPartCode.LeftArm, [new(ItemFlagCode.Arm),]),
			rightArm = new(BodyPartCode.RightArm, [new(ItemFlagCode.Arm),]),
			torso = new(BodyPartCode.Torso, []),
			leftLeg = new(BodyPartCode.LeftLeg, []),
			rightLeg = new(BodyPartCode.RightLeg, []),
		];
	}
	public Character(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			name = reader.ReadString();
			speed = new(reader);
			actionPoint = new(reader);
			inventory = new();
			bodyParts =
			[
				head = new(BodyPartCode.Head, []),
				leftArm = new(BodyPartCode.LeftArm, [new(ItemFlagCode.Arm),]),
				rightArm = new(BodyPartCode.RightArm, [new(ItemFlagCode.Arm),]),
				torso = new(BodyPartCode.Torso, []),
				leftLeg = new(BodyPartCode.LeftLeg, []),
				rightLeg = new(BodyPartCode.RightLeg, []),
			];
			foreach (var bodyPart in bodyParts) bodyPart.Deserialize(reader);
			inventory.Deserialize(reader);
			reaction = 1;
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(name);
			speed.Serialize(writer);
			actionPoint.Serialize(writer);
			foreach (var bodyPart in bodyParts) bodyPart.Serialize(writer);
			inventory.Serialize(writer);
		}
	}
}
