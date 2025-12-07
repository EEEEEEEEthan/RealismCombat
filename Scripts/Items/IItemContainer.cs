public interface IItemContainer : IBuffOwner
{
	ItemSlot[] Slots { get; }
	bool HasBuff(BuffCode buff, bool recursive);
}
