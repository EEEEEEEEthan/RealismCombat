using System.Collections.Generic;
public interface IItemContainer : IBuffOwner
{
	ItemSlot[] Slots { get; }
	public void AppendEquippedItemNames(List<string> parts)
	{
		foreach (var slot in Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			parts.Add(item.IconTag);
		}
	}
}
