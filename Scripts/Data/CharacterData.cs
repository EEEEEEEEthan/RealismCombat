using System.Collections.Generic;
using System.IO;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
public class CharacterData
{
	public readonly string name;
	public readonly byte team;
	public readonly BodyPartData head = new(BodyPartCode.Head);
	public readonly BodyPartData chest = new(BodyPartCode.Chest);
	public readonly BodyPartData leftArm = new(BodyPartCode.LeftArm);
	public readonly BodyPartData rightArm = new(BodyPartCode.RightArm);
	public readonly BodyPartData leftLeg = new(BodyPartCode.LeftLeg);
	public readonly BodyPartData rightLeg = new(BodyPartCode.RightLeg);
	public double actionPoint;
	public double speed = 5;
	public bool PlayerControlled => team == 0;
	public bool Dead => head.hp <= 0 || chest.hp <= 0;
	public IEnumerable<BodyPartData> BodyParts
	{
		get
		{
			yield return head;
			yield return chest;
			yield return leftArm;
			yield return rightArm;
			yield return leftLeg;
			yield return rightLeg;
		}
	}
	public CharacterData(string name, byte team)
	{
		this.name = name;
		this.team = team;
	}
	public CharacterData(DataVersion version, BinaryReader reader)
	{
		name = reader.ReadString();
		team = reader.ReadByte();
		actionPoint = reader.ReadDouble();
		speed = reader.ReadDouble();
		head = new(version: version, reader: reader);
		chest = new(version: version, reader: reader);
		leftArm = new(version: version, reader: reader);
		rightArm = new(version: version, reader: reader);
		leftLeg = new(version: version, reader: reader);
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(name);
		writer.Write(team);
		writer.Write(actionPoint);
		writer.Write(speed);
		head.Serialize(writer);
		chest.Serialize(writer);
		leftArm.Serialize(writer);
		rightArm.Serialize(writer);
		leftLeg.Serialize(writer);
		rightLeg.Serialize(writer);
	}
	public override string ToString() =>
		$"{nameof(CharacterData)}({nameof(name)}={name}, {nameof(team)}={team}, {nameof(actionPoint)}={actionPoint}, {nameof(speed)}={speed}, {nameof(head)}={head}, {nameof(chest)}={chest}, {nameof(leftArm)}={leftArm}, {nameof(rightArm)}={rightArm}, {nameof(leftLeg)}={leftLeg}, {nameof(rightLeg)}={rightLeg})";
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
public class BodyPartData
{
	public readonly BodyPartCode id;
	public int hp = 10;
	public int maxHp = 10;
	public BodyPartData(BodyPartCode id) => this.id = id;
	public BodyPartData(DataVersion version, BinaryReader reader)
	{
		id = (BodyPartCode)reader.ReadByte();
		hp = reader.ReadByte();
		maxHp = reader.ReadByte();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write((byte)id);
		writer.Write(hp);
		writer.Write(maxHp);
	}
	public override string ToString() => $"{nameof(BodyPartData)}({nameof(id)}={id}, {nameof(hp)}={hp}, {nameof(maxHp)}={maxHp})";
}
