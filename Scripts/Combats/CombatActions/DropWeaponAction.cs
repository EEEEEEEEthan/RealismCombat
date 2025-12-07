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
	public static bool IsBodyPartCompatible(BodyPart bodyPart) =>
		bodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	public static bool CanUse(Character actor, BodyPart bodyPart)
	{
		if (!bodyPart.Available) return false;
		if (!IsBodyPartCompatible(bodyPart)) return false;
		return FindWeaponSlot(bodyPart) != null;
	}
	public static DropWeaponAction? Create(Character actor, BodyPart bodyPart, Combat combat)
	{
		var weaponSlot = FindWeaponSlot(bodyPart);
		if (weaponSlot?.Item == null) return null;
		var action = new DropWeaponAction(actor, bodyPart, combat);
		action.SetWeapon(weaponSlot, weaponSlot.Item);
		return action;
	}
	public override bool Available => CanUse(actor, actorBodyPart);
	protected override Task OnStartTask()
	{
		var weaponName = weapon?.Name ?? "武器";
		return DialogueManager.ShowGenericDialogue(startText ?? $"{actor.name}准备丢下{actorBodyPart.Name}上的{weaponName}");
	}
	protected override async Task OnExecute()
	{
		if (weaponSlot?.Item == null) return;
		var droppedWeapon = weaponSlot.Item;
		weaponSlot.Item = null;
		combat.droppedItems.Add(droppedWeapon);
		await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}上的{droppedWeapon.Name}");
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
}

