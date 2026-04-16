@abstract
extends Action
class_name AttackAction

var damage: Damage:
	get: return _get_damage()

func _get_damage() -> Damage:
	push_error("override me")
	return Damage.new(0, 0, 0)

func get_dodge_chance(_from_body: BodyPart, _to_body: BodyPart) -> float:
	push_error("override me")
	return 0

func _get_description() -> String:
	return super._get_description() + &"\n伤害:" + str(damage)
