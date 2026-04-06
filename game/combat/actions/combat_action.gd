@abstract
class_name CombatAction

var from_character: Character
var from_body_part: BodyPartData
var to_character: Character
var to_body_part: BodyPartData

var static_valid: bool:
	# 静态验证。不符合验证的会被剔除
	get:
		return _static_valid()

var dynamic_valid: bool:
	# 动态验证。不符合验证的可显示但不可用
	get:
		return _dynamic_valid()

var valid: bool:
	get:
		return static_valid and dynamic_valid

func _static_valid() -> bool:
	return false

func _dynamic_valid() -> bool:
	return false
