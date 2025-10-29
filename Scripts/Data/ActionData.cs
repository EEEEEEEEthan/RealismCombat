using System.IO;
namespace RealismCombat.Data;
public record ActionData
{
	public readonly int attackerIndex;
	public readonly int defenderIndex;
	public readonly BodyPartCode attackerBody;
	public readonly BodyPartCode defenderBody;
	public ActionData(int attackerIndex, BodyPartCode attackerBody, int defenderIndex, BodyPartCode defenderBody)
	{
		this.attackerIndex = attackerIndex;
		this.defenderIndex = defenderIndex;
		this.attackerBody = attackerBody;
		this.defenderBody = defenderBody;
	}
	public ActionData(DataVersion dataVersion, BinaryReader reader)
	{
		attackerIndex = reader.ReadInt32();
		defenderIndex = reader.ReadInt32();
		attackerBody = (BodyPartCode)reader.ReadByte();
		defenderBody = (BodyPartCode)reader.ReadByte();
	}
	public void Serialize(BinaryWriter writer)
	{
		writer.Write(attackerIndex);
		writer.Write(defenderIndex);
		writer.Write((byte)attackerBody);
		writer.Write((byte)defenderBody);
	}
}
