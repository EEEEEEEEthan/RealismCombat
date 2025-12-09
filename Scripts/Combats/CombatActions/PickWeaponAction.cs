using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
/// <summary>
///     捡起地上武器放到空手上的战斗行动
/// </summary>
public class PickWeaponAction(Character actor, BodyPart actorBodyPart, Combat combat)
	: CombatAction(actor, combat, actorBodyPart, 8, 3)
{
	readonly BodyPart actorBodyPart = actorBodyPart;
	ItemSlot? targetSlot;
	Item? pickedItem;
	string? startText;
	public override CombatActionCode Id => CombatActionCode.PickWeapon;
	public override string Description => "捡起地上的武器放到空闲的手上";
	public override bool Available => IsUsable();
	protected override Task OnStartTask() =>
		DialogueManager.ShowGenericDialogue(startText ?? $"{actor.name}正要捡起武器");
	protected override Task OnExecute()
	{
		if (targetSlot == null || pickedItem == null) return Task.CompletedTask;
		combat.droppedItems.Remove(pickedItem);
		if (targetSlot.Item != null) combat.droppedItems.Add(targetSlot.Item);
		targetSlot.Item = pickedItem;
		return Task.CompletedTask;
	}
	public async Task<bool> PrepareByPlayerSelection()
	{
		targetSlot = FindEmptyHandSlot(actorBodyPart);
		if (targetSlot == null) return false;
		var candidates = GetPickableItems(targetSlot).ToArray();
		if (candidates.Length == 0) return false;
		var options = new MenuOption[candidates.Length];
		for (var i = 0; i < candidates.Length; i++)
		{
			var item = candidates[i];
			options[i] = new()
			{
				title = item.Name,
				description = item.flag.GetDisplayName(),
			};
		}
		var menu = DialogueManager.CreateMenuDialogue("捡起地上武器", true, options);
		var choice = await menu;
		if (choice == options.Length) return false;
		var selected = candidates[choice];
		pickedItem = selected;
		startText = $"{actor.name}弯腰去捡起{selected.Name}";
		return true;
	}
	public bool PrepareByAI()
	{
		targetSlot = FindEmptyHandSlot(actorBodyPart);
		if (targetSlot == null) return false;
		var candidates = GetPickableItems(targetSlot).ToArray();
		if (candidates.Length == 0) return false;
		var index = (int)(GD.Randi() % (uint)candidates.Length);
		var selected = candidates[index];
		pickedItem = selected;
		startText = $"{actor.name}弯腰去捡起{selected.Name}";
		return true;
	}
	IEnumerable<Item> GetPickableItems(ItemSlot slot) =>
		combat.droppedItems.Where(item => (item.flag & slot.Flag) != 0);
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
		var slot = FindEmptyHandSlot(actorBodyPart);
		if (slot == null) return false;
		return GetPickableItems(slot).Any();
	}
}

