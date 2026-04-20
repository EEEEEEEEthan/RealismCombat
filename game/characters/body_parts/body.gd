extends BodyPart
class_name Body

## 身体（原胸部）：可装备腰带，腰带内再放武器。
var belt_slot: ItemSlot


func _init(p_character: Character, hp_current: int, hp_max: int) -> void:
	super(p_character, hp_current, hp_max)
	belt_slot = ItemSlot.new([Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [belt_slot]


func part_name() -> StringName:
	return &"身体"
