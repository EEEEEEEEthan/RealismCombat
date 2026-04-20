extends BodyPart
class_name Foot

var _side: Defs.Side
var footwear_slot: ItemSlot


func _init(p_character: Character, hp_current: int, hp_max: int, side: Defs.Side) -> void:
	super(p_character, hp_current, hp_max)
	_side = side
	footwear_slot = ItemSlot.new([Footwear])


func get_item_slots() -> Array[ItemSlot]:
	return [footwear_slot]


func part_name() -> StringName:
	match _side:
		Defs.Side.RIGHT:
			return &"右脚"
		_:
			return &"左脚"
