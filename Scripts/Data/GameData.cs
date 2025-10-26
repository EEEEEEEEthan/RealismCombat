using System.IO;
namespace RealismCombat.Data;
class GameData
{
	public GameData() { }
	public GameData(DataVersion version, BinaryReader reader) { }
	public void Serialize(BinaryWriter writer) { }
}
