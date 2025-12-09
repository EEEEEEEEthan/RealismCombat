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
	static IEnumerable<IBuffOwner> EnumerateBuffOwners(IItemContainer container)
	{
		yield return container;
		foreach (var slot in container.Slots)
		{
			var item = slot.Item;
			if (item == null) continue;
			foreach (var owner in EnumerateBuffOwners(item)) yield return owner;
		}
	}
	static IEnumerable<IBuffOwner> EnumerateBuffOwners(Character character)
	{
		foreach (var bodyPart in character.bodyParts)
			foreach (var owner in EnumerateBuffOwners(bodyPart))
				yield return owner;
		foreach (var item in character.inventory.Items)
			foreach (var owner in EnumerateBuffOwners(item))
				yield return owner;
	}
	static string GetOwnerName(Character character, IBuffOwner owner) =>
		owner switch
		{
			BodyPart bodyPart => $"{character.name}的{bodyPart.NameWithEquipments}",
			Item item => $"{character.name}的{item.Name}",
			_ => $"{character.name}的目标",
		};
	public override string Description => "松开擒拿或丢弃手中武器，解除自身施加的束缚效果";
	public override bool Visible => actorBodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	/// <summary>
	///     未擒拿且手上没有武器时禁用
	/// </summary>
	public override bool Disabled =>
		!actorBodyPart.HasBuff(BuffCode.Grappling, true) &&
		FindWeaponSlot(actorBodyPart) == null;
	protected override Task OnStartTask() => Task.CompletedTask;
	protected override async Task OnExecute()
	{
		var released = TryReleaseGrapples(out var grappleSource);
		var freedTargetName = released && grappleSource.HasValue
			? RemoveRestrainedBuff(grappleSource.Value)
			: null;
		var droppedWeapon = TryDropWeapon(out var weapon) ? weapon : null;
		if (released)
		{
			var message = $"{actor.name}松开了手";
			if (freedTargetName != null) message += $"，{freedTargetName}恢复行动";
			if (droppedWeapon != null) message += $"，丢下了{actorBodyPart.Name}的{droppedWeapon.Name}";
			await DialogueManager.ShowGenericDialogue(message);
			return;
		}
		if (droppedWeapon != null)
		{
			await DialogueManager.ShowGenericDialogue($"{actor.name}丢下了{actorBodyPart.Name}的{droppedWeapon.Name}");
			return;
		}
		await DialogueManager.ShowGenericDialogue($"{actor.name}的{actorBodyPart.Name}没有可丢弃的武器");
	}
	bool TryReleaseGrapples(out BuffSource? source)
	{
		var released = false;
		source = null;
		var toRemove = new List<Buff>();
		foreach (var buff in actorBodyPart.Buffs)
			if (buff.code == BuffCode.Grappling)
			{
				toRemove.Add(buff);
				source ??= buff.source;
			}
		foreach (var buff in toRemove)
		{
			actorBodyPart.Buffs.Remove(buff);
			released = true;
		}
		return released;
	}
	string? RemoveRestrainedBuff(BuffSource grappleSource)
	{
		foreach (var owner in EnumerateBuffOwners(grappleSource.Character))
		{
			var toRemove = new List<Buff>();
			foreach (var buff in owner.Buffs)
				if (buff.code == BuffCode.Restrained &&
					buff.source is { } buffSource &&
					ReferenceEquals(buffSource.Character, actor) &&
					ReferenceEquals(buffSource.Target, actorBodyPart))
					toRemove.Add(buff);
			foreach (var buff in toRemove)
			{
				owner.Buffs.Remove(buff);
				return GetOwnerName(grappleSource.Character, owner);
			}
		}
		return null;
	}
	bool TryDropWeapon(out Item? weapon)
	{
		var weaponSlot = FindWeaponSlot(actorBodyPart);
		if (weaponSlot?.Item == null)
		{
			weapon = null;
			return false;
		}
		weapon = weaponSlot.Item;
		weaponSlot.Item = null;
		combat.droppedItems.Add(weapon);
		return true;
	}
}
