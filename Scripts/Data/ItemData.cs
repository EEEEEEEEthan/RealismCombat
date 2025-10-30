using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class ItemData
{
	public readonly string name;
	public int count;
	public ItemData(string name, int count)
	{
		this.name = name;
		this.count = count;
	}
	public ItemData(DataVersion version, BinaryReader reader)
	{
		name = reader.ReadString();
		count = reader.ReadInt32();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(name);
		writer.Write(count);
	}
	public override string ToString() => $"{nameof(ItemData)}({nameof(name)}={name}, {nameof(count)}={count})";
}

