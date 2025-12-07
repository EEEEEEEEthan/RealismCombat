using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     从腰带取武器到手的战斗行为
/// </summary>
public class TakeWeaponAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, actorBodyPart, 2, 1)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	ItemSlot? targetSlot;
	ItemSlot? sourceSlot;
	string? beltName;
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
	public static TakeWeaponAction? Create(Character actor, BodyPart bodyPart, Combat combat)
	{
		var action = new TakeWeaponAction(actor, bodyPart, combat);
		return action.Available ? action : null;
	}
	public override bool Available => IsUsable();
	protected override Task OnStartTask() => DialogueManager.ShowGenericDialogue(startText ?? $"{actor.name}伸手去拿{beltName ?? "腰带"}上的武器到{actorBodyPart.Name}");
	protected override Task OnExecute()
	{
		if (sourceSlot?.Item == null || targetSlot == null) return Task.CompletedTask;
		var weapon = sourceSlot.Item;
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
	public async Task<bool> PrepareByPlayerSelection()
	{
		var candidates = GetBeltWeaponCandidates(actor);
		targetSlot = FindEmptyHandSlot(actorBodyPart);
		if (candidates.Count == 0 || targetSlot == null) return false;
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
		if (choice == options.Length) return false;
		var selected = candidates[choice];
		AssignSlots(targetSlot, selected.Slot, selected.Belt.Name);
		var finalWeaponName = selected.Slot.Item?.Name ?? "武器";
		startText = $"{actor.name}伸手去拿{selected.Belt.Name}上的{finalWeaponName}";
		return true;
	}
	public bool PrepareByAI()
	{
		var candidates = GetBeltWeaponCandidates(actor);
		targetSlot = FindEmptyHandSlot(actorBodyPart);
		if (candidates.Count == 0 || targetSlot == null) return false;
		var index = (int)(GD.Randi() % (uint)candidates.Count);
		var selected = candidates[index];
		var weaponName = selected.Slot.Item?.Name ?? "武器";
		AssignSlots(targetSlot, selected.Slot, selected.Belt.Name);
		startText = $"{actor.name}伸手去拿{selected.Belt.Name}上的{weaponName}";
		return true;
	}
	void AssignSlots(ItemSlot target, ItemSlot source, string belt)
	{
		targetSlot = target;
		sourceSlot = source;
		beltName = belt;
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
			if (slot.Item == null && (slot.Flag & ItemFlagCode.Arm) != 0)
				return slot;
		return null;
	}
	bool IsUsable()
	{
		if (!actorBodyPart.Available) return false;
		if (actorBodyPart.id is not (BodyPartCode.LeftArm or BodyPartCode.RightArm)) return false;
		if (FindEmptyHandSlot(actorBodyPart) == null) return false;
		return GetBeltWeaponCandidates(actor).Count > 0;
	}
}