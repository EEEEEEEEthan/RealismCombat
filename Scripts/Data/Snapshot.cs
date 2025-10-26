using System.IO;
namespace RealismCombat.Data;
record Snapshot
{
	public DataVersion version;
	public Snapshot(BinaryReader reader) => version = new(reader);
	public Snapshot(GameData gameData) => version = DataVersion.newest;
	public void Serialize(BinaryWriter writer) => version.Serialize(writer);
}
