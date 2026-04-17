extends RefCounted
class_name BodyPart

var character: Character
var items: Array[Item] = []
var hp: PropertyInt

func _init(p_character: Character, hp_current: int, hp_max: int) -> void:
	character = p_character
	hp = PropertyInt.new(hp_current, hp_max)


func part_name() -> StringName:
	push_error(&"子类需实现 part_name()")
	return &""
