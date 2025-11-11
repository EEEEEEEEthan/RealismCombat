using System;
using System.IO;
namespace RealismCombat.Items;
public class ItemSlot
{
	readonly ItemFlagCode flag;
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
	public ItemSlot(ItemFlagCode flag) => this.flag = flag;
	public ItemSlot(BinaryReader reader)
	{
		flag = (ItemFlagCode)reader.ReadUInt64();
		var hasItem = reader.ReadBoolean();
		if (hasItem) { }
	}
}
