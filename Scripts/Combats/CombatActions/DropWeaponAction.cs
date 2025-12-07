using System.Threading.Tasks;
/// <summary>
///     丢弃手中武器的战斗行为
/// </summary>
public class DropWeaponAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, actorBodyPart, 1, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	ItemSlot? weaponSlot;
	Item? weapon;
	string? startText;
public override string Description => "丢弃当前手上武器到地面，腾出手部装备槽";
	public override bool Available => IsUsable();
	protected override Task OnStartTask()
	{
		var slot = ResolveWeaponSlot();
		if (slot?.Item == null)
			return DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}没有可丢弃的武器");
		var weaponName = slot.Item?.Name ?? "武器";
		return DialogueManager.ShowGenericDialogue(startText ?? $"{actor.name}准备丢下{actorBodyPart.Name}上的{weaponName}");
	}
	protected override async Task OnExecute()
	{
		var slot = ResolveWeaponSlot();
		if (slot?.Item == null)
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}没有可丢弃的武器");
			return;
		}
		var droppedWeapon = slot.Item;
		slot.Item = null;
		combat.droppedItems.Add(droppedWeapon);
		await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}上的{droppedWeapon.Name}");
	}
	ItemSlot? ResolveWeaponSlot()
	{
		weaponSlot ??= FindWeaponSlot(actorBodyPart);
		if (weaponSlot?.Item != null)
			weapon = weaponSlot.Item;
		else
			weapon = null;
		return weaponSlot;
	}
	public void SetWeapon(ItemSlot slot, Item value)
	{
		weaponSlot = slot;
		weapon = value;
		startText = $"{actor.name}准备丢下{actorBodyPart.Name}上的{value.Name}";
	}
	static ItemSlot? FindWeaponSlot(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item != null)
				return slot;
		return null;
	}
	bool IsUsable()
	{
		if (!actorBodyPart.Available) return false;
		if (actorBodyPart.id is not (BodyPartCode.LeftArm or BodyPartCode.RightArm)) return false;
		return FindWeaponSlot(actorBodyPart)?.Item != null;
	}
}

