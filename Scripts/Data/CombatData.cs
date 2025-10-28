using System.Collections.Generic;
using System.IO;
namespace RealismCombat.Data;
class CombatData
{
	public readonly List<CharacterData> characters = [];
	public readonly Dictionary<string, string> tempData = new();
	public byte state;
	public double tickTimer;
	public long tickCount;
	public CombatData(DataVersion version, BinaryReader reader)
	{
		state = reader.ReadByte();
		tickTimer = reader.ReadDouble();
		tickCount = reader.ReadInt64();
		{
			var count = reader.ReadByte();
			for (var i = count; i-- > 0;) characters.Add(new(version: version, reader: reader));
		}
		{
			var count = reader.ReadByte();
			for (var i = 0; i < count; ++i) tempData[reader.ReadString()] = reader.ReadString();
		}
	}
	public CombatData() { }
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(state);
		writer.Write(tickTimer);
		writer.Write(tickCount);
		writer.Write((byte)characters.Count);
		foreach (var character in characters) character.Serialize(writer);
		writer.Write(tempData.Count);
		foreach ((var key, var value) in tempData)
		{
			writer.Write(key);
			writer.Write(value);
		}
	}
}
