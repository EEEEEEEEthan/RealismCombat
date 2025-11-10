using System;
using System.Collections.Generic;
using System.IO;
using RealismCombat.Combats;
using RealismCombat.Extensions;
namespace RealismCombat.Characters;
public class Character
{
	[Obsolete] public readonly PropertyInt hp;
	public readonly PropertyInt speed;
	public readonly PropertyDouble actionPoint;
	public readonly string name;
	public readonly BodyPart head;
	public readonly BodyPart leftArm;
	public readonly BodyPart rightArm;
	public readonly BodyPart torso;
	public readonly BodyPart leftLeg;
	public readonly BodyPart rightLeg;
	public IReadOnlyList<BodyPart> bodyParts;
	public CombatAction? combatAction;
	public bool IsAlive => hp.value > 0;
	public Character(string name)
	{
		this.name = name;
		hp = new(10, 10);
		speed = new(5, 5);
		actionPoint = new(0f, 10f);
		bodyParts =
		[
			head = new(BodyPartCode.Head),
			leftArm = new(BodyPartCode.LeftArm),
			rightArm = new(BodyPartCode.RightArm),
			torso = new(BodyPartCode.Torso),
			leftLeg = new(BodyPartCode.LeftLeg),
			rightLeg = new(BodyPartCode.RightLeg),
		];
	}
	public Character(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			name = reader.ReadString();
			hp = new(reader);
			speed = new(reader);
			actionPoint = new(reader);
			bodyParts =
			[
				head = new(reader), leftArm = new(reader), rightArm = new(reader), torso = new(reader), leftLeg = new(reader), rightLeg = new(reader),
			];
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(name);
			hp.Serialize(writer);
			speed.Serialize(writer);
			actionPoint.Serialize(writer);
			foreach (var bodyPart in bodyParts) bodyPart.Serialize(writer);
		}
	}
}
