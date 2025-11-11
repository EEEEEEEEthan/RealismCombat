using System;
using System.IO;
using RealismCombat.Combats;
using RealismCombat.Extensions;
namespace RealismCombat.Items;
public enum ItemIdCode
{
	LongSword,
}
/// <summary>
///     战斗中可以被选择的装备实体
/// </summary>
public abstract class Item(ItemIdCode id, ItemFlagCode flag, PropertyInt hitPoint) : ICombatTarget
{
	public readonly ItemFlagCode flag = flag;
	public readonly ItemIdCode id = id;
	/// <summary>
	///     目标是否仍具备有效状态
	/// </summary>
	public bool Available => HitPoint.value > 0;
	/// <summary>
	///     装备的耐久属性
	/// </summary>
	public PropertyInt HitPoint { get; } = hitPoint;
	/// <summary>
	///     目标在日志或界面上的名称
	/// </summary>
	public abstract string Name { get; }
	protected abstract void OnSerialize(BinaryWriter writer);
	protected abstract void OnDeserialize(BinaryReader reader);
	#region serialize
	public static Item Load(BinaryReader reader)
	{
		using var _ = reader.ReadScope();
		var id = (ItemIdCode)reader.ReadUInt64();
		Item item = id switch
		{
			ItemIdCode.LongSword => new LongSword(),
			_ => throw new NotSupportedException($"unexpected id: {id}"),
		};
		item.OnDeserialize(reader);
		return item;
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		writer.Write((ulong)id);
		OnSerialize(writer);
	}
	#endregion
}
