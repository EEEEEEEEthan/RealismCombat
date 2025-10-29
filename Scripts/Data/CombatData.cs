using System.Collections.Generic;
using System.IO;
namespace RealismCombat.Data;
public class CombatData
{
	public readonly List<CharacterData> characters = [];
	public readonly Dictionary<string, string> extraData = new();
	public byte state;
	public byte currentCharacterIndex;
	public double tickTimer;
	public long tickCount;
	public ActionData? lastAction;
	public CombatData(DataVersion version, BinaryReader reader)
	{
		state = reader.ReadByte();
		currentCharacterIndex = reader.ReadByte();
		tickTimer = reader.ReadDouble();
		tickCount = reader.ReadInt64();
		{
			var count = reader.ReadByte();
			for (var i = count; i-- > 0;) characters.Add(new(version: version, reader: reader));
		}
		{
			var count = reader.ReadByte();
			for (var i = 0; i < count; ++i) extraData[reader.ReadString()] = reader.ReadString();
		}
		lastAction = new(dataVersion: version, reader: reader);
	}
	public CombatData() { }
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(state);
		writer.Write(currentCharacterIndex);
		writer.Write(tickTimer);
		writer.Write(tickCount);
		writer.Write((byte)characters.Count);
		foreach (var character in characters) character.Serialize(writer);
		writer.Write(extraData.Count);
		foreach ((var key, var value) in extraData)
		{
			writer.Write(key);
			writer.Write(value);
		}
		lastAction.Serialize(writer);
	}
}
