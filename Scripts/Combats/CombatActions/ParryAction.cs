using System.Linq;
using System.Threading.Tasks;
/// <summary>
///     招架行动，前摇1后摇3，招架触发时+1反应
/// </summary>
public sealed class ParryAction(Character actor, BodyPart actorBodyPart, Combat combat) : CombatAction(actor, combat, actorBodyPart, 1, 3)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	public override string Description => "摆出招架姿态，招架触发时+1反应";
	public override bool Visible
	{
		get
		{
			if (!actorBodyPart.Available) return false;
			if (!actorBodyPart.id.IsLeg) return false;
			// 倒伏或被束缚时不可用
			if (actor.bodyParts.Any(part => part.HasBuff(BuffCode.Prone, false))) return false;
			if (actorBodyPart.HasBuff(BuffCode.Restrained, true)) return false;
			return true;
		}
	}
	public CombatActionCode Id => CombatActionCode.Parry;
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue($"{actor.name}摆出招架姿态");
	protected override async Task OnExecute()
	{
		actor.reaction += 1;
		await DialogueManager.ShowGenericDialogue($"{actor.name}摆出架势");
	}
}
