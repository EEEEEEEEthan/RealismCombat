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
	public readonly PropertyInt hp;
	public BodyPart() => hp = new(10, 10);
	public BodyPart(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			hp = new(reader);
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			hp.Serialize(writer);
		}
	}
}
