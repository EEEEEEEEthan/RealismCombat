extends Node
class_name BodyPart

@export var part: Defs.BodyPart
var hp: Property = Property.new(0, 0)

@warning_ignore("unused_private_class_variable")
@export var _hp: Vector2i:
	get:
		return Vector2i(int(hp.value), int(hp.max_value))
	set(v):
		hp.max_value = v.y
		hp.value = v.x
