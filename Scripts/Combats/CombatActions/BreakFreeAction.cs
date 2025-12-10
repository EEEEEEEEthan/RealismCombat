using System.Threading.Tasks;
using Godot;
/// <summary>
///     抽出行动，用于尝试摆脱束缚
/// </summary>
public class BreakFreeAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, actorBodyPart, 2, 1)
{
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
		owner.Buffs.TryGetValue(code, out var buff);
		return buff;
	}
	static bool ContainsBuff(IBuffOwner owner, Buff target)
	{
		if (owner.Buffs.TryGetValue(target.code, out var buff))
			return ReferenceEquals(buff, target);
		return false;
	}
	static string GetOwnerName(IBuffOwner owner) =>
		owner switch
		{
			BodyPart bodyPart => bodyPart.NameWithEquipments,
			Item item => item.Name,
			_ => "目标",
		};
	readonly BodyPart actorBodyPart = actorBodyPart;
	IBuffOwner? buffOwner;
	Buff? restrainedBuff;
	string? targetName;
	public override string Description => "尝试解除自身或装备上的束缚状态，成功时移除束缚";
	public override bool Visible
	{
		get
		{
			RefreshContext();
			return actorBodyPart.Available && buffOwner != null && restrainedBuff != null;
		}
	}
	public virtual CombatActionCode Id => CombatActionCode.BreakFree;
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}正试摆脱{targetName ?? "目标"}");
	protected override async Task OnExecute()
	{
		if (buffOwner == null || restrainedBuff == null)
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}没有束缚需要解除");
			return;
		}
		var hasBuff = ContainsBuff(buffOwner, restrainedBuff);
		var success = hasBuff && GD.Randf() < 0.5f;
		if (success)
		{
			buffOwner.Buffs.Remove(BuffCode.Restrained);
			await DialogueManager.ShowGenericDialogue($"{actor.name}成功摆脱了束缚");
			return;
		}
		if (!hasBuff)
		{
			await DialogueManager.ShowGenericDialogue($"{targetName ?? "目标"}身上的束缚已经消失");
			return;
		}
		await DialogueManager.ShowGenericDialogue($"{actor.name}未能摆脱{targetName ?? "目标"}");
	}
	void RefreshContext()
	{
		var result = FindRestrainedBuff(actorBodyPart);
		if (result == null) return;
		buffOwner = result.Value.Owner;
		restrainedBuff = result.Value.Buff;
		targetName = result.Value.TargetName;
	}
}
