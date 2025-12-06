using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     从腰带取武器到手的战斗行为
/// </summary>
public class TakeWeaponAction(Character actor, BodyPart actorBodyPart, ItemSlot targetSlot, ItemSlot sourceSlot, string beltName, Combat combat)
	: CombatAction(actor, combat, 2, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	readonly ItemSlot targetSlot = targetSlot;
	readonly ItemSlot sourceSlot = sourceSlot;
	readonly string beltName = beltName;
	string? startText;
	class BeltWeaponCandidate
	{
		public BeltWeaponCandidate(Item belt, ItemSlot slot)
		{
			Belt = belt;
			Slot = slot;
		}
		public Item Belt { get; }
		public ItemSlot Slot { get; }
	}
	public static bool IsBodyPartCompatible(BodyPart bodyPart) =>
		bodyPart.id is BodyPartCode.LeftArm or BodyPartCode.RightArm;
	public static bool CanUse(Character actor, BodyPart bodyPart)
	{
		if (!bodyPart.Available) return false;
		if (!IsBodyPartCompatible(bodyPart)) return false;
		if (FindEmptyHandSlot(bodyPart) == null) return false;
		return GetBeltWeaponCandidates(actor).Count > 0;
	}
	public static async Task<CombatAction?> CreateByPlayerSelection(Character actor, BodyPart bodyPart, Combat combat)
	{
		var candidates = GetBeltWeaponCandidates(actor);
		var targetSlot = FindEmptyHandSlot(bodyPart);
		if (candidates.Count == 0 || targetSlot == null) return null;
		var options = new MenuOption[candidates.Count];
		for (var i = 0; i < candidates.Count; i++)
		{
			var candidate = candidates[i];
			var weaponName = candidate.Slot.Item?.Name ?? "未知武器";
			options[i] = new()
			{
				title = weaponName,
				description = $"来源: {candidate.Belt.Name}",
			};
		}
		var menu = DialogueManager.CreateMenuDialogue("选择腰带武器", true, options);
		var choice = await menu;
		if (choice == options.Length) return null;
		var selected = candidates[choice];
		var finalWeaponName = selected.Slot.Item?.Name ?? "武器";
		return new TakeWeaponAction(actor, bodyPart, targetSlot, selected.Slot, selected.Belt.Name, combat)
		{
			startText = $"{actor.name}伸手去拿{selected.Belt.Name}上的{finalWeaponName}",
		};
	}
	public static CombatAction? CreateByAI(Character actor, BodyPart bodyPart, Combat combat)
	{
		var candidates = GetBeltWeaponCandidates(actor);
		var targetSlot = FindEmptyHandSlot(bodyPart);
		if (candidates.Count == 0 || targetSlot == null) return null;
		var index = (int)(GD.Randi() % (uint)candidates.Count);
		var selected = candidates[index];
		var weaponName = selected.Slot.Item?.Name ?? "武器";
		return new TakeWeaponAction(actor, bodyPart, targetSlot, selected.Slot, selected.Belt.Name, combat)
		{
			startText = $"{actor.name}伸手去拿{selected.Belt.Name}上的{weaponName}",
		};
	}
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue(startText ?? $"{actor.name}伸手去拿{beltName}上的武器到{actorBodyPart.Name}");
	protected override Task OnExecute()
	{
		var weapon = sourceSlot.Item;
		if (weapon == null) return Task.CompletedTask;
		if (targetSlot.Item != null)
		{
			var dropped = targetSlot.Item;
			targetSlot.Item = null;
			combat.droppedItems.Add(dropped);
		}
		targetSlot.Item = weapon;
		sourceSlot.Item = null;
		return Task.CompletedTask;
	}
	static List<BeltWeaponCandidate> GetBeltWeaponCandidates(Character actor)
	{
		var result = new List<BeltWeaponCandidate>();
		foreach (var bodyPart in actor.bodyParts) CollectBeltWeapons(bodyPart, result);
		return result;
	}
	static void CollectBeltWeapons(IItemContainer container, List<BeltWeaponCandidate> result)
	{
		foreach (var slot in container.Slots)
		{
			if (slot.Item == null) continue;
			var item = slot.Item;
			if ((item.flag & ItemFlagCode.Belt) != 0)
			{
				foreach (var beltSlot in item.Slots)
					if (beltSlot.Item is { flag: var flag } && (flag & ItemFlagCode.Arm) != 0)
						result.Add(new(item, beltSlot));
			}
			CollectBeltWeapons(item, result);
		}
	}
	static ItemSlot? FindEmptyHandSlot(BodyPart bodyPart)
	{
		foreach (var slot in bodyPart.Slots)
			if (slot.Item == null)
				return slot;
		return null;
	}
}

