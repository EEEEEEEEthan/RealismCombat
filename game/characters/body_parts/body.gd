extends BodyPart
class_name Body

## 身体：可直接穿外套、内衬或仅系腰带；中层/外层护甲叠穿须从内衬起向上逐层穿。
var torso_slot: ItemSlot


func _init(p_character: Character, hp_current: int, hp_max: int) -> void:
	super(p_character, hp_current, hp_max)
	torso_slot = ItemSlot.new([InnerLiningGarment, OuterwearCoat, Belt])


func get_item_slots() -> Array[ItemSlot]:
	return [torso_slot]


func part_name() -> StringName:
	return &"身体"
