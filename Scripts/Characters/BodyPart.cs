using System.IO;
using RealismCombat.Combats;
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
public class BodyPart : ICombatTarget
{
	public readonly BodyPartCode id;
	public readonly PropertyInt hp;
	/// <summary>
	///     目标是否仍具备有效状态
	/// </summary>
	public bool IsTargetAlive => hp.value > 0;
	/// <summary>
	///     目标的生命值属性
	/// </summary>
	public PropertyInt HitPoint => hp;
	/// <summary>
	///     目标在日志或界面上的名称
	/// </summary>
	public string TargetName => this.GetName();
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
/// <summary>
///     身体部位扩展工具
/// </summary>
public static class BodyPartExtensions
{
	/// <summary>
	///     获取身体部位的显示名称
	/// </summary>
	public static string GetName(this BodyPart bodyPart) =>
		bodyPart.id switch
		{
			BodyPartCode.Head => "头部",
			BodyPartCode.LeftArm => "左臂",
			BodyPartCode.RightArm => "右臂",
			BodyPartCode.Torso => "躯干",
			BodyPartCode.LeftLeg => "左腿",
			BodyPartCode.RightLeg => "右腿",
			_ => bodyPart.id.ToString(),
		};
}
