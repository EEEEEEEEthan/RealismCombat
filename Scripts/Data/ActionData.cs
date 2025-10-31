using System.Collections.Generic;
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
public class ActionConfig
{
	public static readonly Dictionary<ActionCode, ActionConfig> Configs = new();
	static ActionConfig()
	{
		Configs[ActionCode.StraightPunch] = new(actionCode: ActionCode.StraightPunch, damageRange: (1, 3), actionPointCost: 5, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.HookPunch] = new(actionCode: ActionCode.HookPunch, damageRange: (2, 4), actionPointCost: 6, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Swing] = new(actionCode: ActionCode.Swing, damageRange: (3, 5), actionPointCost: 7, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Thrust] = new(actionCode: ActionCode.Thrust, damageRange: (2, 6), actionPointCost: 6, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
		Configs[ActionCode.Kick] = new(actionCode: ActionCode.Kick, damageRange: (4, 8), actionPointCost: 8, allowedBodyParts: new[] { BodyPartCode.LeftLeg, BodyPartCode.RightLeg });
		Configs[ActionCode.ElbowStrike] = new(actionCode: ActionCode.ElbowStrike, damageRange: (2, 4), actionPointCost: 4, allowedBodyParts: new[] { BodyPartCode.LeftArm, BodyPartCode.RightArm });
	}
	public (int min, int max) damageRange { get; private set; }
	public int actionPointCost { get; private set; }
	public ActionCode actionCode { get; private set; }
	public IReadOnlyList<BodyPartCode> allowedBodyParts { get; private set; }
	ActionConfig(ActionCode actionCode, (int min, int max) damageRange, int actionPointCost, IReadOnlyList<BodyPartCode> allowedBodyParts)
	{
		this.actionCode = actionCode;
		this.damageRange = damageRange;
		this.actionPointCost = actionPointCost;
		this.allowedBodyParts = allowedBodyParts;
	}
}
