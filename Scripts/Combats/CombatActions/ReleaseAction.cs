using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
///     放手行动，可解除擒拿或丢弃武器
/// </summary>
public class ReleaseAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, 1, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	public static bool IsBodyPartCompatible(BodyPart bodyPart) => bodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	public static bool CanUse(Character actor, BodyPart bodyPart, Combat combat) =>
		bodyPart.Available && IsBodyPartCompatible(bodyPart) && (HasGrapplingBuff(bodyPart) || FindWeaponSlot(bodyPart) != null);
	public static ReleaseAction? Create(Character actor, BodyPart bodyPart, Combat combat)
	{
		if (!CanUse(actor, bodyPart, combat)) return null;
		return new ReleaseAction(actor, bodyPart, combat);
	}
	protected override Task OnStartTask()
	{
		if (HasGrapplingBuff(actorBodyPart)) return DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}准备放手");
		var weaponSlot = FindWeaponSlot(actorBodyPart);
		var weaponName = weaponSlot?.Item?.Name ?? "武器";
		return DialogueManager.ShowGenericDialogue($"{actor.name}准备丢下{actorBodyPart.Name}上的{weaponName}");
	}
	protected override async Task OnExecute()
	{
		var released = RemoveGrapplingBuffs(actorBodyPart);
		if (released)
		{
			var freedCount = RemoveRestrainedBuffsFromOthers();
			var message = $"{actor.name}松开了手";
			if (freedCount > 0) message += $"，{freedCount}个目标恢复行动";
			await DialogueManager.ShowGenericDialogue(message);
			return;
		}
		var weaponSlot = FindWeaponSlot(actorBodyPart);
		if (weaponSlot?.Item == null)
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}没有可丢弃的武器");
			return;
		}
		var droppedWeapon = weaponSlot.Item;
		weaponSlot.Item = null;
		combat.droppedItems.Add(droppedWeapon);
		await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}上的{droppedWeapon.Name}");
	}
	static bool HasGrapplingBuff(IItemContainer container)
	{
		if (container is IBuffOwner owner)
		{
			foreach (var buff in owner.Buffs)
				if (buff.code == BuffCode.Grappling)
					return true;
		}
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			if (HasGrapplingBuff(item)) return true;
		}
		return false;
	}
	bool RemoveGrapplingBuffs(IItemContainer container)
	{
		var removed = false;
		if (container is IBuffOwner owner)
		{
			var toRemove = new List<Buff>();
			foreach (var buff in owner.Buffs)
				if (buff.code == BuffCode.Grappling)
					toRemove.Add(buff);
			foreach (var buff in toRemove)
			{
				owner.RemoveBuff(buff);
				removed = true;
			}
		}
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			if (RemoveGrapplingBuffs(item)) removed = true;
		}
		return removed;
	}
	int RemoveRestrainedBuffsFromOthers()
	{
		var freedCount = 0;
		foreach (var character in combat.Allies)
		{
			freedCount += RemoveRestrainedBuffs(character);
		}
		foreach (var character in combat.Enemies)
		{
			freedCount += RemoveRestrainedBuffs(character);
		}
		return freedCount;
	}
	int RemoveRestrainedBuffs(Character target)
	{
		var freedCount = 0;
		foreach (var bodyPart in target.bodyParts)
		{
			freedCount += RemoveRestrainedBuffs(bodyPart);
			foreach (var slot in bodyPart.Slots)
			{
				var item = slot.Item;
				if (item == null) continue;
				freedCount += RemoveRestrainedBuffs(item);
			}
		}
		foreach (var item in target.inventory.Items)
		{
			freedCount += RemoveRestrainedBuffs(item);
		}
		return freedCount;
	}
	int RemoveRestrainedBuffs(IItemContainer container)
	{
		var freedCount = 0;
		if (container is IBuffOwner owner)
		{
			var toRemove = new List<Buff>();
			foreach (var buff in owner.Buffs)
				if (buff.code == BuffCode.Restrained && buff.source == actor)
					toRemove.Add(buff);
			foreach (var buff in toRemove)
			{
				owner.RemoveBuff(buff);
				freedCount++;
			}
		}
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			freedCount += RemoveRestrainedBuffs(item);
		}
		return freedCount;
	}
	static ItemSlot? FindWeaponSlot(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item != null)
				return slot;
		return null;
	}
}


