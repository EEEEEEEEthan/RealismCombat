using System.Threading.Tasks;
/// <summary>
///     丢弃手中武器的战斗行为
/// </summary>
public class DropWeaponAction(Character actor, BodyPart actorBodyPart, ItemSlot weaponSlot, Item weapon, Combat combat)
	: CombatAction(actor, combat, 1, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	readonly ItemSlot weaponSlot = weaponSlot;
	readonly Item weapon = weapon;
	readonly string startText = $"{actor.name}准备丢下{actorBodyPart.Name}上的{weapon.Name}";
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
		return new(actor, bodyPart, weaponSlot, weaponSlot.Item, combat);
	}
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue(startText);
	protected override async Task OnExecute()
	{
		if (weaponSlot.Item == null) return;
		var droppedWeapon = weaponSlot.Item;
		weaponSlot.Item = null;
		combat.droppedItems.Add(droppedWeapon);
		await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}上的{droppedWeapon.Name}");
	}
	static ItemSlot? FindWeaponSlot(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item != null)
				return slot;
		return null;
	}
}

