using System.Collections.Generic;

public interface IItemContainer
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
