using System;
using System.IO;
public class ItemSlot(ItemFlagCode flag, IItemContainer container)
{
	public readonly IItemContainer container = container ?? throw new ArgumentNullException(nameof(container));
	public IItemContainer Container => container;
	/// <summary>
	///     槽位可接受的装备类型
	/// </summary>
	public ItemFlagCode Flag => flag;
	public Item? Item
	{
		get;
		set
		{
			if (value != null)
				if ((value.flag & flag) == 0)
					throw new ArgumentException("装备类型不匹配!");
			field = value;
		}
	}
	public void Deserialize(BinaryReader reader)
	{
		using var _ = reader.ReadScope();
		var hasItem = reader.ReadBoolean();
		if (hasItem) Item = Item.Load(reader);
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		if (Item != null)
		{
			writer.Write(true);
			Item.Serialize(writer);
		}
		else
		{
			writer.Write(false);
		}
	}
}
