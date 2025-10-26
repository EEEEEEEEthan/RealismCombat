using System.IO;
namespace RealismCombat.Data;
public readonly struct DataVersion(ulong version)
{
	public static readonly DataVersion newest = new(major: 0, minor: 0, build: 1);
	public readonly ulong version = version;
	public uint Build => (uint)(version & uint.MaxValue);
	public ushort Minor => (ushort)((version >> 32) & ushort.MaxValue);
	public ushort Major => (ushort)(version >> 48);
	public DataVersion(ushort major, ushort minor, uint build) : this(((ulong)major << 48) | ((ulong)minor << 32) | build) { }
	public DataVersion(BinaryReader reader) : this(reader.ReadUInt64()) { }
	public void Serialize(BinaryWriter writer) => writer.Write(version);
}
