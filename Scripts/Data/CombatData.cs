using System.Collections.Generic;
using System.IO;
namespace RealismCombat.Data;
class CombatData
{
	public readonly List<CharacterData> characters = new();
	public byte state;
	public CombatData(DataVersion version, BinaryReader reader)
	{
		state = reader.ReadByte();
		var count = reader.ReadByte();
		for (var i = count; i-- > 0;) characters.Add(new(version: version, reader: reader));
	}
	public CombatData() { }
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(state);
		writer.Write((byte)characters.Count);
		foreach (var character in characters) character.Serialize(writer);
	}
}
