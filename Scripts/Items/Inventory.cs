using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Items;
/// <summary>
///     物品栏，仅用于持有物品列表
/// </summary>
public class Inventory
{
	public List<Item> Items { get; } = new();
	public void Deserialize(BinaryReader reader)
	{
		using var _ = reader.ReadScope();
		var count = reader.ReadInt32();
		Items.Clear();
		for (var i = 0; i < count; i++)
		{
			var item = Item.Load(reader);
			Items.Add(item);
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using var _ = writer.WriteScope();
		writer.Write(Items.Count);
		foreach (var item in Items) item.Serialize(writer);
	}
}


