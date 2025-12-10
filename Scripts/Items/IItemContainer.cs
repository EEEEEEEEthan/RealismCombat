using System.Collections.Generic;
public interface IItemContainer : IBuffOwner
{
	ItemSlot[] Slots { get; }
}
public static class ItemContainerExtensions
{
	extension(IItemContainer container)
	{
		/// <summary>
		///     递归累加容器及其子节点上的所有装备重量
		/// </summary>
		public double WeightRecursive
		{
			get
			{
				var total = 0.0;
				foreach (var slot in container.Slots)
				{
					if (slot.Item == null) continue;
					total += slot.Item.Weight;
					if (slot.Item.Slots.Length > 0) total += slot.Item.WeightRecursive;
				}
				return total;
			}
		}
		/// <summary>
		///     递归计算容器内装备重量
		/// </summary>
		public double ContainerWeight
		{
			get
			{
				var total = 0.0;
				foreach (var slot in container.Slots)
				{
					if (slot.Item == null) continue;
					total += slot.Item.Weight;
					if (slot.Item.Slots.Length > 0) total += slot.Item.ContainerWeight;
				}
				return total;
			}
		}
		/// <summary>
		///     检查容器是否自由（无束缚、无擒拿、血量未归零）
		/// </summary>
		public bool Free
		{
			get
			{
				if (container is ICombatTarget target && !target.Available) return false;
				if (container.HasBuff(BuffCode.Restrained, false)) return false;
				if (container.HasBuff(BuffCode.Grappling, false)) return false;
				foreach (var slot in container.Slots)
				{
					var item = slot.Item;
					if (item == null) continue;
					if (!item.Free) return false;
				}
				return true;
			}
		}
		public bool HasBuff(BuffCode buff, bool recursive)
		{
			if (container.Buffs.ContainsKey(buff))
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
		public IEnumerable<Item> IterItems(ItemFlagCode flags)
		{
			foreach (var slot in container.Slots)
			{
				var occupied = slot.Item;
				if (occupied == null) continue;
				if ((occupied.flag & flags) != 0) yield return occupied;
				foreach (var nested in occupied.IterItems(flags)) yield return nested;
			}
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
		public IEnumerable<(Item item, ItemSlot slot)> IterItemAndSlots(ItemFlagCode flags)
		{
			foreach (var slot in container.Slots)
			{
				if (slot.Item == null) continue;
				var item = slot.Item;
				foreach (var s in item.Slots)
					if (s.Item is { flag: var flag, } && (flag & flags) != 0)
						yield return (item, s);
				foreach (var pair in item.IterItemAndSlots(flags)) yield return pair;
			}
		}
	}
}
