using System;
using System.IO;
public readonly struct GameVersion : IEquatable<GameVersion>
{
	public static readonly GameVersion newest = new(0, 0, 2);
	public static bool operator ==(GameVersion a, GameVersion b) => a.value == b.value;
	public static bool operator !=(GameVersion a, GameVersion b) => a.value != b.value;
	public static bool operator <(GameVersion a, GameVersion b) => a.value < b.value;
	public static bool operator >(GameVersion a, GameVersion b) => a.value > b.value;
	public static bool operator <=(GameVersion a, GameVersion b) => a.value <= b.value;
	public static bool operator >=(GameVersion a, GameVersion b) => a.value >= b.value;
	readonly ulong value;
	public ushort Major => (ushort)((value >> 48) & ushort.MaxValue);
	public ushort Minor => (ushort)((value >> 32) & ushort.MaxValue);
	public uint Build => (uint)(value & uint.MaxValue);
	public GameVersion(ushort major, ushort minor, uint build) : this(((ulong)major << 48) | ((ulong)minor << 32) | build) { }
	public GameVersion(BinaryReader reader) : this(reader.ReadUInt64()) { }
	GameVersion(ulong value) => this.value = value;
	public override bool Equals(object? obj) => obj is GameVersion other && this == other;
	public override int GetHashCode() => value.GetHashCode();
	public override string ToString() => $"{Major}.{Minor}.{Build}";
	public void Serialize(BinaryWriter writer) => writer.Write(value);
	public bool Equals(GameVersion other) => value == other.value;
}
