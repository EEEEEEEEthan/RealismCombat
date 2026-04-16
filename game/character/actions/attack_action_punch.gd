extends AttackAction
class_name AttackActionPunch

var _damage: Damage = Damage.new(0, 0, 1)

func _init(chr: Character) -> void:
	super(chr)

func _get_damage() -> Damage:
	return _damage

func get_weight(from_body: BodyPart, to_body: BodyPart) -> float:
	var dmg = damage.sum
	var weight = pow(1 - get_dodge_chance(from_body, to_body) * dmg, 2)
	# 如果这一击能把部位打烂，权重应该翻倍
	if dmg >= to_body.hp.value:
		weight += weight
	return weight

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
	return &"一种几乎本能的徒手攻击\n" + super._get_description()

func _get_windup_action_points() -> int:
	return 2

func _get_recovery_action_points() -> int:
	return 3

func get_dodge_chance(_from_body: BodyPart, to_body: BodyPart) -> float:
	match to_body.part:
		Defs.BodyPart.HEAD:
			return 0.9
		Defs.BodyPart.CHEST:
			return 0.4
		Defs.BodyPart.RIGHT_HAND:
			return 0.7
		Defs.BodyPart.LEFT_HAND:
			return 0.7
		Defs.BodyPart.RIGHT_FOOT:
			return 0.9
		Defs.BodyPart.LEFT_FOOT:
			return 0.9
	return 0


func preview(from_body: BodyPart, to_body: BodyPart) -> String:
	var dodge_chance = get_dodge_chance(from_body, to_body)
	return &"闪避成功率" + str(int(dodge_chance * 100)) + &"%"

func execute(from_body: BodyPart, to_body: BodyPart) -> void:
	await super.execute(from_body, to_body)
	var combat := from_body.character.game.combat
	var attacker_renderer := combat.get_character_renderer(from_body.character)
	var defender_renderer := combat.get_character_renderer(to_body.character)
	await attacker_renderer.animate_generic_attack()
	var hit_chance := 1 - get_dodge_chance(from_body, to_body)
	var menu = Dialogues.create_generic_dialogue()
	if randf() < hit_chance:
		await combat.hit_stop(0.2)
		var d = _damage.sum
		to_body.hp.value -= d
		AudioManager.play_hit()
		defender_renderer.animate_generic_hit()
		menu.text = &"造成伤害:" + str(_damage)
	else:
		defender_renderer.animate_generic_dodge()
		menu.text = &"未命中"
	await menu.pressed
	menu.queue_free()
