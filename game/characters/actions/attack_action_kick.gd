extends AttackAction
class_name AttackActionKick

var _damage: Damage = Damage.new(0, 0, 2)

func _init(chr: Character) -> void:
	super(chr)

func static_valid_from_body(from_body: BodyPart) -> Outcome:
	if not (from_body is Foot):
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_from_body(from_body: BodyPart) -> Outcome:
	if from_body.hp.value <= 0:
		return Outcome.from_failure("%s无法行动" % from_body.part_name())
	return Outcome.from_success()

func static_valid_to_body(to_body: BodyPart) -> Outcome:
	if not to_body.character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_to_body(to_body: BodyPart) -> Outcome:
	if to_body.hp.value <= 0:
		return Outcome.from_failure("%s早已无法行动" % to_body.part_name())
	return Outcome.from_success()

func dynamic_valid_to_character(to_character: Character) -> Outcome:
	if not to_character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func get_dodge_chance(_from_body: BodyPart, to_body: BodyPart) -> float:
	if to_body is Head:
		return 0.9
	if to_body is Body:
		return 0.4
	if to_body is Hand:
		return 0.7
	if to_body is Foot:
		return 0.9
	return 0

func get_damage() -> Damage:
	return _damage

func get_name() -> StringName:
	return &"踢"

func get_description() -> String:
	return "用脚踢击\n%s" % super.get_description()

func get_windup_action_points() -> int:
	return 2

func get_recovery_action_points() -> int:
	return 3
