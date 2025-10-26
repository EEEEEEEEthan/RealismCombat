using System.IO;
namespace RealismCombat.Data;
class GameData
{
	public byte state;
	public GameData() { }
	public GameData(DataVersion version, BinaryReader reader) => state = reader.ReadByte();
	public void Serialize(BinaryWriter writer) => writer.Write(state);
}
