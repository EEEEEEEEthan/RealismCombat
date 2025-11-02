using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class CombatData
{
	public readonly List<CharacterData> characters = [];
	public byte state;
	public double tickTimer;
	public long tickCount;
	public byte[]? stateData;
	public CombatData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			state = reader.ReadByte();
			tickTimer = reader.ReadDouble();
			tickCount = reader.ReadInt64();
			var count = reader.ReadByte();
			for (var i = count; i-- > 0;) characters.Add(new(version: version, reader: reader));
			var stateDataLength = reader.ReadInt32();
			if (stateDataLength < 0)
				stateData = null;
			else if (stateDataLength == 0)
				stateData = [];
			else
				stateData = reader.ReadBytes(stateDataLength);
		}
	}
	public CombatData() { }
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(state);
			writer.Write(tickTimer);
			writer.Write(tickCount);
			writer.Write((byte)characters.Count);
			foreach (var character in characters) character.Serialize(writer);
			if (stateData == null)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write(stateData.Length);
				if (stateData.Length > 0) writer.Write(stateData);
			}
		}
	}
}
