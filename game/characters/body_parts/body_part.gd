extends RefCounted
class_name BodyPart

var character: Character
var hp: PropertyInt

func _init(p_character: Character, hp_current: int, hp_max: int) -> void:
	character = p_character
	hp = PropertyInt.new(hp_current, hp_max)


func part_name() -> StringName:
	push_error(&"子类需实现 part_name()")
	return &""


func get_item_slots() -> Array[ItemSlot]:
	return []


func serialize(file_access: FileAccess) -> void:
	file_access.store_8(hp.max_value)
	file_access.store_8(hp.value)


func deserialize(file_access: FileAccess) -> void:
	hp.max_value = file_access.get_8()
	hp.value = file_access.get_8()
