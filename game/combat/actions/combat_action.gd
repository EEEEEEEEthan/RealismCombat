@abstract
class_name CombatAction

var from_character: Character
var from_body_part: BodyPartData
var to_character: Character
var to_body_part: BodyPartData

var valid: bool:
	get:
		return _valid()

func _valid() -> bool:
	return false
