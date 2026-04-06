extends CombatAction
class_name ActionPunch

var _AVAILABLE = [Defs.BodyPart.LEFT_HAND, Defs.BodyPart.RIGHT_HAND]

func _static_valid() -> bool:
	return from_body_part.part in _AVAILABLE
