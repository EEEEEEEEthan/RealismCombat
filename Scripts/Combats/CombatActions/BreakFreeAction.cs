using System.Threading.Tasks;
using Godot;
/// <summary>
///     抽出行动，用于尝试摆脱束缚
/// </summary>
public class BreakFreeAction(Character actor, BodyPart actorBodyPart, IBuffOwner buffOwner, Buff restrainedBuff, string targetName, Combat combat)
	: CombatAction(actor, combat, 2, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	readonly IBuffOwner buffOwner = buffOwner;
	readonly Buff restrainedBuff = restrainedBuff;
	readonly string targetName = targetName;
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => FindRestrainedBuff(bodyPart) != null;
	public static bool CanUse(Character actor, BodyPart bodyPart) => bodyPart.Available && IsBodyPartCompatible(bodyPart);
	public static BreakFreeAction? Create(Character actor, BodyPart bodyPart, Combat combat)
	{
		if (!bodyPart.Available) return null;
		var result = FindRestrainedBuff(bodyPart);
		if (result == null) return null;
		return new BreakFreeAction(actor, bodyPart, result.Value.Owner, result.Value.Buff, result.Value.TargetName, combat);
	}
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}正试摆脱{targetName}");
	protected override async Task OnExecute()
	{
		var hasBuff = ContainsBuff(buffOwner, restrainedBuff);
		var success = hasBuff && GD.Randf() < 0.5f;
		if (success)
		{
			buffOwner.RemoveBuff(restrainedBuff);
			await DialogueManager.ShowGenericDialogue($"{actor.name}成功摆脱了束缚");
			return;
		}
		if (!hasBuff)
		{
			await DialogueManager.ShowGenericDialogue($"{targetName}身上的束缚已经消失");
			return;
		}
		await DialogueManager.ShowGenericDialogue($"{actor.name}未能摆脱{targetName}");
	}
	static (IBuffOwner Owner, Buff Buff, string TargetName)? FindRestrainedBuff(IItemContainer container)
	{
		if (container is IBuffOwner owner)
		{
			var buff = FindBuff(owner, BuffCode.Restrained);
			if (buff != null) return (owner, buff, GetOwnerName(owner));
		}
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			var child = FindRestrainedBuff(item);
			if (child != null) return child;
		}
		return null;
	}
	static Buff? FindBuff(IBuffOwner owner, BuffCode code)
	{
		foreach (var buff in owner.Buffs)
			if (buff.code == code)
				return buff;
		return null;
	}
	static bool ContainsBuff(IBuffOwner owner, Buff target)
	{
		foreach (var buff in owner.Buffs)
			if (ReferenceEquals(buff, target))
				return true;
		return false;
	}
	static string GetOwnerName(IBuffOwner owner) =>
		owner switch
		{
			BodyPart bodyPart => bodyPart.GetNameWithEquipments(),
			Item item => item.Name,
			_ => "目标",
		};
}


