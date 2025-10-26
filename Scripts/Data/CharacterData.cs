using System.IO;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
class CharacterData
{
	/// <summary>
	///     角色名字
	/// </summary>
	public readonly string name;
	/// <summary>
	///     角色队伍
	/// </summary>
	public readonly byte team;
	public CharacterData(string name, byte team)
	{
		this.name = name;
		this.team = team;
	}
	public CharacterData(DataVersion version, BinaryReader reader)
	{
		name = reader.ReadString();
		team = reader.ReadByte();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(name);
		writer.Write(team);
	}
}
