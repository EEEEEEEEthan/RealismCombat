using System.Collections.Generic;
public interface IItemContainer : IBuffOwner
{
	ItemSlot[] Slots { get; }
}
public static class ItemContainerExtensions
{
	extension(IItemContainer container)
	{
		public bool HasBuff(BuffCode buff, bool recursive)
		{
			foreach (var owned in container.Buffs)
				if (owned.code == buff)
					return true;
			if (!recursive) return false;
			foreach (var slot in container.Slots)
			{
				var item = slot.Item;
				if (item == null) continue;
				if (item.HasBuff(buff, true)) return true;
			}
			return false;
		}
		public bool TryGetItem(ItemFlagCode flag, out Item item)
		{
			foreach (var slot in container.Slots)
			{
				var occupied = slot.Item;
				if (occupied != null && (occupied.flag & flag) != 0)
				{
					item = occupied;
					return true;
				}
			}
			item = null!;
			return false;
		}
		public bool RemoveItem(Item item)
		{
			foreach (var slot in container.Slots)
			{
				if (ReferenceEquals(slot.Item, item))
				{
					slot.Item = null;
					return true;
				}
				var occupied = slot.Item;
				if (occupied != null && occupied.RemoveItem(item)) return true;
			}
			return false;
		}
		public IEnumerable<(Item, ItemSlot)> IterItems(ItemFlagCode flags)
		{
			foreach (var slot in container.Slots)
			{
				if (slot.Item == null) continue;
				var item = slot.Item;
				foreach (var s in item.Slots)
					if (s.Item is { flag: var flag, } && (flag & flags) != 0)
						yield return (item, s);
				foreach (var pair in item.IterItems(flags)) yield return pair;
			}
		}
	}
}
