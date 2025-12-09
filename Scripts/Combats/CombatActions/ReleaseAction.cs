using System.Collections.Generic;
using System.Threading.Tasks;
/// <summary>
///     放手行动，可解除擒拿或丢弃武器
/// </summary>
public sealed class ReleaseAction(Character actor, BodyPart actorBodyPart, Combat combat) : CombatAction(actor, combat, actorBodyPart, 0, 1)
{
	static ItemSlot? FindWeaponSlot(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
		{
			if (slot.Item == null) continue;
			if ((slot.Flag & ItemFlagCode.Arm) == 0) continue;
			return slot;
		}
		return null;
	}
	public override string Description => "松开擒拿或丢弃手中武器，解除自身施加的束缚效果";
	public override bool Visible => IsUsable();
	/// <summary>
	///     判断当前是否只会执行丢弃武器
	/// </summary>
	public bool WillOnlyDropWeapon => !actorBodyPart.HasBuff(BuffCode.Grappling, true) && FindWeaponSlot(actorBodyPart) != null;
	protected override Task OnStartTask() => Task.CompletedTask;
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
		await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}的{droppedWeapon.Name}");
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
				owner.Buffs.Remove(buff);
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
		foreach (var character in combat.Allies) freedCount += RemoveRestrainedBuffs(character);
		foreach (var character in combat.Enemies) freedCount += RemoveRestrainedBuffs(character);
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
		foreach (var item in target.inventory.Items) freedCount += RemoveRestrainedBuffs(item);
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
				owner.Buffs.Remove(buff);
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
	bool IsUsable()
	{
		if (!actorBodyPart.Available) return false;
		if (actorBodyPart.id is not (BodyPartCode.LeftArm or BodyPartCode.RightArm)) return false;
		return actorBodyPart.HasBuff(BuffCode.Grappling, true) || FindWeaponSlot(actorBodyPart) != null;
	}
}
