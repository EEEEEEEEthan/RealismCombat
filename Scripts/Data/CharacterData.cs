using System.IO;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
class CharacterData
{
	public readonly string name;
	public readonly byte team;
	public readonly BodyPart head = new(BodyPartCode.Head);
	public readonly BodyPart chest = new(BodyPartCode.Chest);
	public readonly BodyPart leftArm = new(BodyPartCode.LeftArm);
	public readonly BodyPart rightArm = new(BodyPartCode.RightArm);
	public readonly BodyPart leftLeg = new(BodyPartCode.LeftLeg);
	public readonly BodyPart rightLeg = new(BodyPartCode.RightLeg);
	public double actionPoint;
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
		head.Serialize(writer);
		chest.Serialize(writer);
		leftArm.Serialize(writer);
		rightArm.Serialize(writer);
		leftLeg.Serialize(writer);
		rightLeg.Serialize(writer);
	}
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
class BodyPart
{
	public readonly BodyPartCode bodyPart;
	public int hp = 10;
	public BodyPart(BodyPartCode bodyPart) => this.bodyPart = bodyPart;
	public BodyPart(DataVersion version, BinaryReader reader)
	{
		bodyPart = (BodyPartCode)reader.ReadByte();
		hp = reader.ReadByte();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write((byte)bodyPart);
		writer.Write(hp);
	}
}
