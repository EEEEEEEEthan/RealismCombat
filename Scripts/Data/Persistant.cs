using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
static class Persistant
{
	public const string saveDataPath = "save.dat";
	public static void Save(GameData data, string path)
	{
		var snapshot = new Snapshot(data);
		var stream = new FileStream(path: path, mode: FileMode.Create);
		var writer = new BinaryWriter(stream);
		using (writer.WriteScope())
		{
			snapshot.Serialize(writer);
		}
		data.Serialize(writer);
		Log.Print("游戏已储存");
	}
	public static Snapshot LoadSnapshot(string path)
	{
		var stream = new FileStream(path: path, mode: FileMode.Open);
		var reader = new BinaryReader(stream);
		using (reader.ReadScope())
		{
			return new(reader);
		}
	}
	public static GameData Load(string path)
	{
		var stream = new FileStream(path: path, mode: FileMode.Open);
		var reader = new BinaryReader(stream);
		Snapshot snapshot;
		using (reader.ReadScope())
		{
			snapshot = new(reader);
		}
		return new(version: snapshot.version, reader: reader);
	}
}
