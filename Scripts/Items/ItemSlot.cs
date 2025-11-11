using System;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Items;
public class ItemSlot(ItemFlagCode flag)
{
	Item? item;
	public Item? Item
	{
		get => item;
		set
		{
			if (value != null)
				if ((value.flag & flag) == 0)
					throw new ArgumentException("装备类型不匹配!");
			item = value;
		}
	}
	public void Deserialize(BinaryReader reader)
	{
		using var _ = reader.ReadScope();
		var hasItem = reader.ReadBoolean();
		if (hasItem) item = Item.Load(reader);
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		if (item != null)
		{
			writer.Write(true);
			item.Serialize(writer);
		}
		else
		{
			writer.Write(false);
		}
	}
}
