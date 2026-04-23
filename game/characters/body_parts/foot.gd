extends BodyPart
class_name Foot

var side: Defs.Side
var footwear_slot: ItemSlot


func _init(p_character: Character, hp_current: int, hp_max: int, p_side: Defs.Side) -> void:
	super(p_character, hp_current, hp_max)
	side = p_side
	footwear_slot = ItemSlot.new([Footwear])
	item_slots = [footwear_slot]


func part_name() -> StringName:
	match side:
		Defs.Side.RIGHT:
			return &"右脚"
		_:
			return &"左脚"
