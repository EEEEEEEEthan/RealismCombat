extends Node
class_name BodyPart

static var _hands = [Defs.BodyPart.LEFT_HAND, Defs.BodyPart.RIGHT_HAND]
static var _feet = [Defs.BodyPart.LEFT_HAND, Defs.BodyPart.RIGHT_HAND]

@onready var character: Character = get_parent()

@export var part: Defs.BodyPart
var hp: Property = Property.new(0, 0)

@warning_ignore("unused_private_class_variable")
@export var _hp: Vector2i:
	get:
		return Vector2i(int(hp.value), int(hp.max_value))
	set(v):
		hp.max_value = v.y
		hp.value = v.x

var is_hand: bool:
	get: return part in _hands

var is_foot: bool:
	get: return part in _feet

var part_name: StringName:
	get: return Defs.get_body_part_name(part)
