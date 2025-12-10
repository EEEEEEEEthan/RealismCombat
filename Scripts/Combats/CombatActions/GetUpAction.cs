using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
/// <summary>
///     爬起行动，用于解除倒伏状态
/// </summary>
public sealed class GetUpAction(Character actor, BodyPart actorBodyPart, Combat combat) : CombatAction(actor, combat, actorBodyPart, 5, 5)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	public override string Description => "尝试从倒伏状态中爬起，解除倒伏效果";
	public override bool Visible
	{
		get
		{
			if (actorBodyPart.id is not (BodyPartCode.LeftArm or BodyPartCode.RightArm)) return false;
			return actor.bodyParts.Any(part => part.HasBuff(BuffCode.Prone, false));
		}
	}
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}正试图爬起");
	protected override async Task OnExecute()
	{
		var removedCount = 0;
		foreach (var bodyPart in actor.bodyParts)
		{
			var toRemove = bodyPart.Buffs.Where(buff => buff.code == BuffCode.Prone).ToList();
			foreach (var buff in toRemove)
			{
				bodyPart.Buffs.Remove(buff);
				removedCount++;
			}
		}
		if (removedCount > 0)
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}成功爬起，解除了倒伏状态");
		}
		else
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}没有倒伏状态需要解除");
		}
	}
}

