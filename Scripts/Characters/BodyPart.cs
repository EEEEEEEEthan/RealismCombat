using System.IO;
using Godot;
using RealismCombat.Combats;
using RealismCombat.Extensions;
using RealismCombat.Items;
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
/// <summary>
///     身体部位扩展工具
/// </summary>
public static class BodyPartExtensions
{
	/// <summary>
	///     获取身体部位的显示名称
	/// </summary>
	public static string GetName(this BodyPartCode @this) =>
		@this switch
		{
			BodyPartCode.Head => "头部",
			BodyPartCode.LeftArm => "左臂",
			BodyPartCode.RightArm => "右臂",
			BodyPartCode.Torso => "躯干",
			BodyPartCode.LeftLeg => "左腿",
			BodyPartCode.RightLeg => "右腿",
			_ => @this.ToString(),
		};
}
public class BodyPart : ICombatTarget, IItemContainer
{
	public readonly BodyPartCode id;
	/// <summary>
	///     目标是否仍具备有效状态
	/// </summary>
	public bool Available => HitPoint.value > 0;
	/// <summary>
	///     目标的生命值属性
	/// </summary>
	public PropertyInt HitPoint { get; }
	/// <summary>
	///     目标在日志或界面上的名称
	/// </summary>
	public string Name => id.GetName();
	public ItemSlot[] Slots { get; }
	public BodyPart(BodyPartCode id, ItemSlot[] slots)
	{
		this.id = id;
		var maxHitPoint = id switch
		{
			BodyPartCode.Head => 5,
			BodyPartCode.LeftArm or BodyPartCode.RightArm => 8,
			BodyPartCode.LeftLeg or BodyPartCode.RightLeg => 8,
			_ => 10,
		};
		HitPoint = new(maxHitPoint, maxHitPoint);
		Slots = slots;
	}
	public void Deserialize(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			HitPoint.Deserialize(reader);
			var count = reader.ReadInt32();
			var trueCount = Mathf.Min(count, Slots.Length);
			for (var i = 0; i < trueCount; i++) Slots[i].Deserialize(reader);
			for (var i = trueCount; i < count; i++) new ItemSlot(default).Deserialize(reader);
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			HitPoint.Serialize(writer);
			writer.Write(Slots.Length);
			foreach (var slot in Slots) slot.Serialize(writer);
		}
	}
}
