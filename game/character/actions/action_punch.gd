extends Action
class_name ActionPunch

func _init(p_character: Character) -> void:
	super(p_character)

func get_weight(_from_body: BodyPart, _to_body: BodyPart) -> float:
	return 1

func static_valid_from_body(from_body: BodyPart) -> Outcome:
	if not from_body.is_hand:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_from_body(from_body: BodyPart) -> Outcome:
	if from_body.hp.value <= 0:
		return Outcome.from_failure(from_body.part_name + &"无法行动")
	return Outcome.from_success()

func static_valid_to_body(to_body: BodyPart) -> Outcome:
	if not to_body.character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_to_body(to_body: BodyPart) -> Outcome:
	if to_body.hp.value <= 0:
		return Outcome.from_failure(to_body.part_name + &"早已无法行动")
	return Outcome.from_success()

func static_valid_to_character(to_character: Character) -> Outcome:
	if to_character == character:
		return Outcome.from_failure(&"不能选择自己")
	if not to_character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_to_character(to_character: Character) -> Outcome:
	if not to_character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func _get_name() -> StringName:
	return &"直拳"

func _get_description() -> String:
	return &"一种几乎本能的徒手攻击"

func _get_windup_action_points() -> int:
	return 2

func _get_recovery_action_points() -> int:
	return 3

func execute(from_body: BodyPart, to_body: BodyPart) -> void:
	await super.execute(from_body, to_body)
	var combat := from_body.character.game.combat
	var attacker_renderer := combat.get_character_renderer(from_body.character)
	var defender_renderer := combat.get_character_renderer(to_body.character)
	await attacker_renderer.animate_generic_attack()
	defender_renderer.animate_generic_hit()
	var menu = Dialogues.create_generic_dialogue()
	menu.text = &"伤害结算"
	await menu.pressed
	menu.queue_free()
