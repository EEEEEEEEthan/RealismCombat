@abstract
class_name CombatAction

var from_character: Character
var from_body_part: BodyPartData
var to_character: Character
var to_body_part: BodyPartData

var static_validate: CombatActionError:
	# 静态验证。不符合验证的会被剔除
	get:
		return _static_validate()

var dynamic_validate: CombatActionError:
	# 动态验证。不符合验证的可显示但不可用
	get:
		return _dynamic_validate()

var validate: CombatActionError:
	get:
		var s = static_validate
		if s:
			return s
		var d = dynamic_validate
		if d:
			return d
		return null

func _static_validate() -> CombatActionError:
	return CombatActionError.new(CombatActionError.ErrorCode.ABSTRACT_CLASS)

func _dynamic_validate() -> CombatActionError:
	return CombatActionError.new(CombatActionError.ErrorCode.ABSTRACT_CLASS)
