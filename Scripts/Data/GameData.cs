using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class GameData
{
	public byte state;
	public CombatData? combatData;
	public readonly List<ItemData> items = [];
	public GameData()
	{
		items.Add(new(itemId: 0, count: 100));
	}
	public GameData(DataVersion version, BinaryReader reader)
	{
		state = reader.ReadByte();
		var hasCombatData = reader.ReadBoolean();
		if (hasCombatData)
		{
			using (reader.ReadScope())
			{
				combatData = new(version: version, reader: reader);
			}
		}
		{
			using (reader.ReadScope())
			{
				var count = reader.ReadInt32();
				for (var i = 0; i < count; ++i)
				{
					items.Add(new(version: version, reader: reader));
				}
			}
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(state);
		if (combatData is not null)
		{
			writer.Write(true);
			using (writer.WriteScope())
			{
				combatData.Serialize(writer);
			}
		}
		else
		{
			writer.Write(false);
		}
		using (writer.WriteScope())
		{
			writer.Write(items.Count);
			foreach (var item in items)
			{
				item.Serialize(writer);
			}
		}
	}
}
