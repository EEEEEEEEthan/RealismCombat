extends BodyPart
class_name Hand

var _side: Defs.Side
var glove_slot: ItemSlot
var weapon_slot: ItemSlot


func _init(p_character: Character, hp_current: int, hp_max: int, side: Defs.Side) -> void:
	super(p_character, hp_current, hp_max)
	_side = side
	glove_slot = ItemSlot.new([Glove])
	weapon_slot = ItemSlot.new([Weapon])


func get_item_slots() -> Array[ItemSlot]:
	return [glove_slot, weapon_slot]


func part_name() -> StringName:
	match _side:
		Defs.Side.RIGHT:
			return &"右手"
		_:
			return &"左手"
