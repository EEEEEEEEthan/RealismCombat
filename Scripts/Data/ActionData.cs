using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Data;
public record ActionData
{
	public readonly int attackerIndex;
	public readonly int defenderIndex;
	public readonly BodyPartCode attackerBody;
	public readonly BodyPartCode defenderBody;
	public readonly ActionCode actionCode;
	public ActionData(int attackerIndex, BodyPartCode attackerBody, int defenderIndex, BodyPartCode defenderBody, ActionCode actionCode)
	{
		this.attackerIndex = attackerIndex;
		this.defenderIndex = defenderIndex;
		this.attackerBody = attackerBody;
		this.defenderBody = defenderBody;
		this.actionCode = actionCode;
	}
	public ActionData(DataVersion dataVersion, BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			attackerIndex = reader.ReadInt32();
			defenderIndex = reader.ReadInt32();
			attackerBody = (BodyPartCode)reader.ReadByte();
			defenderBody = (BodyPartCode)reader.ReadByte();
			actionCode = (ActionCode)reader.ReadByte();
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(attackerIndex);
			writer.Write(defenderIndex);
			writer.Write((byte)attackerBody);
			writer.Write((byte)defenderBody);
			writer.Write((byte)actionCode);
		}
	}
}
public enum ActionCode
{
	StraightPunch,
	HookPunch,
	Swing,
	Thrust,
	Kick,
	ElbowStrike,
}
public static class ActionCodeExtensions
{
	public static string GetName(this ActionCode motion) =>
		motion switch
		{
			ActionCode.StraightPunch => "冲拳",
			ActionCode.HookPunch => "勾拳",
			ActionCode.Swing => "挥砍",
			ActionCode.Thrust => "刺击",
			ActionCode.Kick => "踢",
			ActionCode.ElbowStrike => "肘击",
			_ => "未知动作",
		};
}
