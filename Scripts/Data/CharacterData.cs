using System.IO;
namespace RealismCombat.Data;
/// <summary>
///     角色类（占位），承载角色的基本信息
/// </summary>
class CharacterData(string name, byte team, double actionPoint)
{
	public readonly string name = name;
	public readonly byte team = team;
	public double actionPoint = actionPoint;
	public CharacterData(DataVersion version, BinaryReader reader) :
		this(name: reader.ReadString(), team: reader.ReadByte(), actionPoint: reader.ReadDouble()) { }
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(name);
		writer.Write(team);
		writer.Write(actionPoint);
	}
}
