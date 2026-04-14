extends RefCounted
class_name BodyPart

static var _hands = [Defs.BodyPart.LEFT_HAND, Defs.BodyPart.RIGHT_HAND]
static var _feet = [Defs.BodyPart.LEFT_FOOT, Defs.BodyPart.RIGHT_FOOT]

var character: Character
var part: Defs.BodyPart
var hp: Property = Property.new(0, 0)

func _init(p_character: Character, p_part: Defs.BodyPart, hp_current: float, hp_max: float) -> void:
	character = p_character
	part = p_part
	hp.value = hp_current
	hp.max_value = hp_max

var is_hand: bool:
	get:
		return part in _hands

var is_foot: bool:
	get:
		return part in _feet

var part_name: StringName:
	get:
		return Defs.get_body_part_name(part)
