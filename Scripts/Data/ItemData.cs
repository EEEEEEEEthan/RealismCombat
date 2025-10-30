using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class ItemData
{
	public readonly uint itemId;
	public int count;
	public ItemData(uint itemId, int count)
	{
		this.itemId = itemId;
		this.count = count;
	}
	public ItemData(DataVersion version, BinaryReader reader)
	{
		itemId = reader.ReadUInt32();
		count = reader.ReadInt32();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(itemId);
		writer.Write(count);
	}
	public override string ToString() => $"{nameof(ItemData)}({nameof(itemId)}={itemId}, {nameof(count)}={count})";
}
public record ItemConfig
{
	public uint itemId;
	public required string name;
	public static readonly Dictionary<uint, ItemConfig> Configs = new();
}

