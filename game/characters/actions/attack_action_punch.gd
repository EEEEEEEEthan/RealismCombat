extends AttackAction
class_name AttackActionPunch

var _damage: Damage = Damage.new(0, 0, 1)

func _init(chr: Character) -> void:
	super(chr)

func get_weight(from_body: BodyPart, to_body: BodyPart) -> float:
	var dmg = get_damage().sum
	var weight = pow(1 - get_dodge_chance(from_body, to_body) * dmg, 2)
	var bonus = 1
	# 如果这一击能把部位打烂，权重应该翻倍
	if dmg >= to_body.hp.value: bonus += 1
	return weight * bonus

func static_valid_from_body(from_body: BodyPart) -> Outcome:
	if not (from_body is Hand):
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_from_body(from_body: BodyPart) -> Outcome:
	if from_body.hp.value <= 0:
		return Outcome.from_failure(from_body.part_name() + &"无法行动")
	return Outcome.from_success()

func static_valid_to_body(to_body: BodyPart) -> Outcome:
	if not to_body.character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_to_body(to_body: BodyPart) -> Outcome:
	if to_body.hp.value <= 0:
		return Outcome.from_failure(to_body.part_name() + &"早已无法行动")
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
	return &"直拳"

func get_description() -> String:
	return &"一种几乎本能的徒手攻击\n" + super.get_description()

func get_windup_action_points() -> int:
	return 2

func get_recovery_action_points() -> int:
	return 3
