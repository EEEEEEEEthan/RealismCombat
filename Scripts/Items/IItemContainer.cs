public interface IItemContainer : IBuffOwner
{
	ItemSlot[] Slots { get; }
}
public static class ItemContainerExtensions
{
	public static bool HasBuff(this IItemContainer container, BuffCode buff, bool recursive)
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
}
