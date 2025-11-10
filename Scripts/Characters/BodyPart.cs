using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Characters;
/// <summary>
///     战斗中的身体部位类型
/// </summary>
public enum BodyPartCode
{
	Head,
	LeftArm,
	RightArm,
	Torso,
	LeftLeg,
	RightLeg,
}
public class BodyPart
{
	public readonly BodyPartCode id;
	public readonly PropertyInt hp;
	public BodyPart() : this(BodyPartCode.Head) { }
	public BodyPart(BodyPartCode id)
	{
		this.id = id;
		hp = new(10, 10);
	}
	public BodyPart(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			id = (BodyPartCode)reader.ReadInt32();
			hp = new(reader);
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write((int)id);
			hp.Serialize(writer);
		}
	}
}
