using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public class GameData
{
	public byte state;
	public CombatData? combatData;
	public GameData() { }
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
	}
}
