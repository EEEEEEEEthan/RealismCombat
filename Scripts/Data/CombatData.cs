using System.Collections.Generic;
using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class CombatData
{
	public readonly List<CharacterData> characters = [];
	public byte state;
	public byte currentCharacterIndex;
	public double tickTimer;
	public long tickCount;
	public CombatData(DataVersion version, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			using (reader.ReadScope())
			{
				state = reader.ReadByte();
				currentCharacterIndex = reader.ReadByte();
				tickTimer = reader.ReadDouble();
				tickCount = reader.ReadInt64();
			}
			var count = reader.ReadByte();
			for (var i = count; i-- > 0;) characters.Add(new(version: version, reader: reader));
		}
	}
	public CombatData() { }
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			using (writer.WriteScope())
			{
				writer.Write(state);
				writer.Write(currentCharacterIndex);
				writer.Write(tickTimer);
				writer.Write(tickCount);
			}
			writer.Write((byte)characters.Count);
			foreach (var character in characters) character.Serialize(writer);
		}
	}
}
