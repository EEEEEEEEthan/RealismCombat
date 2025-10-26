using System.IO;
namespace RealismCombat.Data;
public readonly struct DataVersion(ulong version)
{
	public readonly ulong version = version;
	public uint Build => (uint)(version & uint.MaxValue);
	public ushort Minor => (ushort)((version >> 32) & ushort.MaxValue);
	public ushort Major => (ushort)(version >> 48);
	public DataVersion(BinaryReader reader) : this(reader.ReadUInt64()) { }
	public void Serialize(BinaryWriter writer) => writer.Write(version);
}
