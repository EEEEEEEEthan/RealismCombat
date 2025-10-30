using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class GameData
{
	public readonly List<ItemData> items = [];
	public byte state;
	public CombatData? combatData;
	public GameData() => items.Add(new(itemId: 0, count: 100));
	public GameData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			state = reader.ReadByte();
			if (reader.ReadBoolean()) combatData = new(version: version, reader: reader);
			var count = reader.ReadInt32();
			for (var i = 0; i < count; ++i) items.Add(new(version: version, reader: reader));
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(state);
			if (combatData is not null)
			{
				writer.Write(true);
				combatData.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}
			writer.Write(items.Count);
			foreach (var item in items) item.Serialize(writer);
		}
	}
}
