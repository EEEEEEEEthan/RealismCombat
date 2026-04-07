@abstract
class_name CombatAction

var from_character: Character
var from_body_part: BodyPartData
var to_character: Character
var to_body_part: BodyPartData

## 依次产出 { "bodypart": BodyPartData, "errormsg": String }；errormsg 为空表示可作发动部位。
func iter_available_from_body_parts() -> Array:
	var pairs: Array = []
	for body_part in from_character.all_body_parts:
		if not _static_validate_from_body_part(body_part):
			continue
		pairs.append({
			"bodypart": body_part,
			"errormsg": _dynamic_validate_from_body_part(body_part),
		})
	return pairs

## 子类覆写：返回false菜单不可见
func _static_validate_from_body_part(body_part: BodyPartData) -> bool:
	return false

## 子类覆写：返回空串表示该部位可用，否则为不可用说明。
func _dynamic_validate_from_body_part(body_part: BodyPartData) -> String:
	return ""
