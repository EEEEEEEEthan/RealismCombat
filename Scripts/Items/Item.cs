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
public abstract class Item(ItemIdCode id, ItemFlagCode flag, ItemSlot[] slots, PropertyInt hitPoint) : ICombatTarget, IItemContainer
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
	public ItemSlot[] Slots { get; } = slots;
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
		var slotCount = reader.ReadInt32();
		var trueSlotCount = Math.Min(slotCount, item.Slots.Length);
		for (var i = 0; i < trueSlotCount; i++) item.Slots[i].Deserialize(reader);
		for (var i = trueSlotCount; i < slotCount; i++) new ItemSlot(default).Deserialize(reader);
		item.OnDeserialize(reader);
		return item;
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		writer.Write((ulong)id);
		writer.Write(Slots.Length);
		for (var i = 0; i < Slots.Length; i++) Slots[i].Serialize(writer);
		OnSerialize(writer);
	}
	#endregion
}
