extends RefCounted
class_name BodyPart

var character: Character
var items: Array[Item] = []
var hp: Property

func _init(p_character: Character, hp_current: float, hp_max: float) -> void:
	character = p_character
	hp = Property.new(hp_current, hp_max)


func part_name() -> StringName:
	push_error(&"子类需实现 part_name()")
	return &""
