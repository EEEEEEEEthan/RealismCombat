using System.Collections.Generic;
using System.IO;
using Godot;
/// <summary>
///     战斗中的身体部位类型
/// </summary>
public enum BodyPartCode
{
	Head,
	LeftArm,
	RightArm,
	Torso,
	Groin,
	LeftLeg,
	RightLeg,
}
/// <summary>
///     身体部位扩展工具
/// </summary>
public static class BodyPartExtensions
{
	extension(BodyPartCode @this)
	{
		/// <summary>
		///     获取身体部位的显示名称
		/// </summary>
		public string Name =>
			@this switch
			{
				BodyPartCode.Head => "头部",
				BodyPartCode.LeftArm => "左臂",
				BodyPartCode.RightArm => "右臂",
				BodyPartCode.Torso => "躯干",
				BodyPartCode.Groin => "裆部",
				BodyPartCode.LeftLeg => "左腿",
				BodyPartCode.RightLeg => "右腿",
				_ => @this.ToString(),
			};
		/// <summary>
		///     当前部位是否为手臂
		/// </summary>
		public bool IsArm => @this is BodyPartCode.LeftArm or BodyPartCode.RightArm;
		/// <summary>
		///     当前部位是否为腿部
		/// </summary>
		public bool IsLeg => @this is BodyPartCode.LeftLeg or BodyPartCode.RightLeg;
		/// <summary>
		///     获取部位在身高中的归一化位置
		/// </summary>
		public double NormalizedHeight =>
			@this switch
			{
				BodyPartCode.Head => 1.0,
				BodyPartCode.Torso => 0.85,
				BodyPartCode.Groin => 0.7,
				BodyPartCode.LeftArm or BodyPartCode.RightArm => 0.75,
				BodyPartCode.LeftLeg or BodyPartCode.RightLeg => 0.35,
				_ => 0.6,
			};
	}
}
public class BodyPart : ICombatTarget, IItemContainer
{
	static int GetMaxHitPoint(BodyPartCode id) =>
		id switch
		{
			BodyPartCode.Head => 5,
			BodyPartCode.LeftArm or BodyPartCode.RightArm => 8,
			BodyPartCode.Groin => 6,
			BodyPartCode.LeftLeg or BodyPartCode.RightLeg => 8,
			_ => 10,
		};
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
	public string Name => id.Name;
	public ItemSlot[] Slots { get; }
	/// <summary>
	///     当前部位是否装备武器
	/// </summary>
	public bool HasWeapon
	{
		get
		{
			foreach (var slot in Slots)
			{
				var item = slot.Item;
				if (item != null && (item.flag & ItemFlagCode.Arm) != 0) return true;
			}
			return false;
		}
	}
	/// <summary>
	///     获取所有Buff列表
	/// </summary>
	public List<Buff> Buffs { get; } = [];
	/// <summary>
	///     获取包含当前装备的身体部位名称
	/// </summary>
	public string NameWithEquipments
	{
		get
		{
			var parts = new List<string> { Name, };
			foreach (var slot in Slots)
			{
				var item = slot.Item;
				if (item == null) continue;
				parts.Add(item.IconTag);
			}
			return string.Concat(parts);
		}
	}
	public BodyPart(BodyPartCode id)
	{
		this.id = id;
		var maxHitPoint = GetMaxHitPoint(id);
		HitPoint = new(maxHitPoint, maxHitPoint);
		Slots = CreateSlots(id);
	}
	public void Deserialize(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			HitPoint.Deserialize(reader);
			var count = reader.ReadInt32();
			var trueCount = Mathf.Min(count, Slots.Length);
			for (var i = 0; i < trueCount; i++) Slots[i].Deserialize(reader);
			for (var i = trueCount; i < count; i++) new ItemSlot(default, this).Deserialize(reader);
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
	ItemSlot[] CreateSlots(BodyPartCode id) =>
		id switch
		{
			BodyPartCode.LeftArm or BodyPartCode.RightArm => [new(ItemFlagCode.HandArmor, this), new(ItemFlagCode.Arm, this, false),],
			BodyPartCode.Groin => [new(ItemFlagCode.LegArmor, this),],
			BodyPartCode.Torso => [new(ItemFlagCode.TorsoArmor, this), new(ItemFlagCode.Belt, this),],
			_ => [],
		};
}
