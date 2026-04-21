@abstract
extends Action
class_name AttackAction

var block_cost: float = 2
var dodge_cost: float = 4

func get_dodge_chance(_from_body: BodyPart, _to_body: BodyPart) -> float:
	push_error(&"override me")
	return 0

func preview(from_body: BodyPart, to_body: BodyPart) -> String:
	var dodge_chance = get_dodge_chance(from_body, to_body)
	return "闪避成功率%d%%" % int(dodge_chance * 100)

func execute(from_body: BodyPart, to_body: BodyPart) -> GenericDialogue:
	var dialogue = await super.execute(from_body, to_body)
	var combat := from_body.character.game.combat
	var attacker_renderer := combat.get_character_renderer(from_body.character)
	await attacker_renderer.animate_generic_attack()
	await _react(from_body, to_body, dialogue)
	return dialogue

func get_damage() -> Damage:
	push_error(&"override me")
	return Damage.new(0, 0, 0)

func get_weight(from_body: BodyPart, to_body: BodyPart) -> float:
	var dmg = get_damage().sum
	var weight = pow(1 - get_dodge_chance(from_body, to_body) * dmg, 2)
	var bonus = 1
	# 如果这一击能把部位打烂，权重应该翻倍
	if dmg >= to_body.hp.value:
		bonus += 1
	return weight * bonus

func static_valid_to_character(to_character: Character) -> Outcome:
	var combat := character.game.combat
	if combat.is_player_character(character) == combat.is_player_character(to_character):
		return Outcome.from_failure(&"不能选择友方")
	if not to_character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func get_description() -> String:
	return "%s\n伤害:%s" % [super.get_description(), get_damage()]

func _react(from_body: BodyPart, to_body: BodyPart, dialogue: GenericDialogue) -> void:
	Engine.time_scale = 0
	var combat := to_body.character.game.combat
	var dodge_chance = get_dodge_chance(from_body, to_body)
	var defender_renderer := combat.get_character_renderer(to_body.character)
	var defender_sm := to_body.character.state_machine
	var dodge_busy := defender_sm.current_state is CharacterStateMachineActionState

	var deliver_damage = func(show_dialogue: bool) -> void:
		await combat.get_tree().create_timer(0.3, true, false, true).timeout  # 根据伤害要有一个顿帧
		Engine.time_scale = 1
		var damage_total = get_damage().sum
		to_body.hp.value -= damage_total
		AudioManager.play_hit()
		if to_body.hp.value < 3:
			defender_renderer.animate_heavy_hit()
		else:
			defender_renderer.animate_generic_hit()
		if show_dialogue:
			dialogue.text += "\n造成伤害:%s" % get_damage()
			await dialogue.pressed

	var dodge = func() -> void:
		defender_sm.action_points.value = maxf(
			0.0,
			defender_sm.action_points.value - dodge_cost,
		)
		if randf() < get_dodge_chance(from_body, to_body):
			Engine.time_scale = 1
			defender_renderer.animate_generic_dodge()
			dialogue.text += "\n%s轻巧地闪开了" % combat.bbcode_character_name(to_body.character)
			AudioManager.play_dodge()
			await dialogue.pressed
		else:
			await deliver_damage.call(false)
			dialogue.text += "\n%s尝试闪避但是失败了.造成伤害:%s" % [
				combat.bbcode_character_name(to_body.character),
				get_damage(),
			]
			await dialogue.pressed

	if combat.characters[to_body.character] == 0:  # player
		var execution_text = get_execution_text(from_body, to_body)
		var menu = Dialogues.create_menu_dialogue()
		menu.title = execution_text
		var busy_name := (
			(defender_sm.current_state as CharacterStateMachineActionState).action_name
			if dodge_busy
			else &""
		)
		if dodge_busy and busy_name.is_empty():
			busy_name = &"动作"
		var dodge_insufficient_action_points := defender_sm.action_points.value < dodge_cost
		var dodge_disabled := dodge_busy or dodge_insufficient_action_points
		var dodge_desc := "成功率%d%% 消耗行动力:%s" % [int(dodge_chance * 100), dodge_cost]
		if dodge_busy:
			dodge_desc = "当前正在%s,不可闪避" % busy_name
		elif dodge_insufficient_action_points:
			dodge_desc = "行动力不足(需要%s)" % dodge_cost
		menu.options = [
			MenuItemData.new(&"闪避", dodge_disabled, dodge_desc),
			MenuItemData.new(&"硬抗"),
		] as Array[MenuItemData]
		var option = await menu.pressed
		menu.queue_free()
		if option == 0:
			await dodge.call()
		else:
			await deliver_damage.call(true)
	else:  # ai
		if dodge_busy or to_body.character.state_machine.action_points.value < dodge_cost:
			await deliver_damage.call(false)
		else:
			await dodge.call()
