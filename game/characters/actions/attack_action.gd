@abstract
extends Action
class_name AttackAction

## 受击触发「重击」时，从防守方状态机扣除的行动力数值（与 UI 播报「{角色}行动力-{本值}」一致）。
## 与闪避消耗等独立结算，结果会经 maxf 夹到 0，不会扣成负数。
const HEAVY_HIT_AP_LOSS := 4.0

var dodge_cost: float = 4

var windup_ticks: float:
	get:
		var combat := character.game.combat
		var per_tick: float = combat.get_battler(character).state_machine.action_points_per_tick
		if per_tick <= 0.0:
			return 0.0
		return float(get_windup_action_points()) / per_tick

func get_dodge_chance(_from_body: BodyPart, _to_body: BodyPart) -> float:
	push_error(&"override me")
	return 0

func preview(from_body: BodyPart, to_body: BodyPart) -> String:
	var dodge_chance = get_dodge_chance(from_body, to_body)
	return "闪避成功率%d%%" % int(dodge_chance * 100)

func execute(from_body: BodyPart, to_body: BodyPart) -> GenericDialogue:
	var dialogue = await super.execute(from_body, to_body)
	var combat := from_body.character.game.combat
	var attacker_renderer := combat.get_character_renderer(combat.get_battler(from_body.character))
	await attacker_renderer.animate_generic_attack()
	await _react(from_body, to_body, dialogue)
	return dialogue

func get_damage() -> Damage:
	push_error(&"override me")
	return Damage.new(0, 0, 0)

func get_weight(from_body: BodyPart, to_body: BodyPart) -> float:
	var damage_total := get_damage().sum
	var weight: float = damage_total
	var combat_ctx := character.game.combat
	var defender_sm: CharacterStateMachine = combat_ctx.get_battler(to_body.character).state_machine
	var cannot_dodge := _defender_cannot_dodge(defender_sm)
	if not cannot_dodge:
		var dodge_chance := get_dodge_chance(from_body, to_body)
		weight *= pow(1.0 - dodge_chance, 2.0)
	if damage_total >= to_body.hp.value:
		weight *= 2.0
	return weight

func static_valid_to_character(to_battler: Battler) -> Outcome:
	var combat := character.game.combat
	var attacker_battler: Battler = combat.get_battler(character)
	if combat.is_player_character(attacker_battler) == combat.is_player_character(to_battler):
		return Outcome.from_failure(&"不能选择友方")
	if not to_battler.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func get_description() -> String:
	return "%s\n伤害:%s" % [super.get_description(), get_damage()]

func _defender_cannot_dodge(defender_sm: CharacterStateMachine) -> bool:
	return defender_sm.current_state is CharacterStateMachineActionState

func _react(from_body: BodyPart, to_body: BodyPart, dialogue: GenericDialogue) -> void:
	Engine.time_scale = 0
	var combat := to_body.character.game.combat
	var defender_battler: Battler = combat.get_battler(to_body.character)
	var dodge_chance = get_dodge_chance(from_body, to_body)
	var defender_renderer := combat.get_character_renderer(defender_battler)
	var defender_sm: CharacterStateMachine = defender_battler.state_machine
	var defender_in_action := defender_sm.current_state is CharacterStateMachineActionState
	var cannot_dodge := _defender_cannot_dodge(defender_sm)

	var deliver_damage = func() -> void:
		await combat.get_tree().create_timer(0.3, true, false, true).timeout
		Engine.time_scale = 1
		var damage_total := get_damage().sum
		var heavy_suffix := ""
		var was_in_action := defender_sm.current_state is CharacterStateMachineActionState
		var interrupted_label := ""
		if was_in_action:
			var interrupted_name := (defender_sm.current_state as CharacterStateMachineActionState).action_name
			interrupted_label = String(interrupted_name)
		to_body.hp.value -= damage_total
		AudioManager.play_hit()
		if to_body.hp.value < 3 or randf() < float(damage_total) / 3.0:
			if was_in_action:
				defender_sm.set_idle()
			defender_sm.action_points.value = maxf(
				0.0,
				defender_sm.action_points.value - HEAVY_HIT_AP_LOSS,
			)
			var defender_bbcode := combat.bbcode_character_name(defender_battler)
			if was_in_action:
				var action_label := interrupted_label
				if action_label.is_empty():
					action_label = "动作"
				heavy_suffix = "重击！%s的%s被打断了！%s行动力-%d" % [
					defender_bbcode,
					action_label,
					defender_bbcode,
					int(HEAVY_HIT_AP_LOSS),
				]
			else:
				heavy_suffix = "重击！%s行动力-%d" % [defender_bbcode, int(HEAVY_HIT_AP_LOSS)]
			defender_renderer.animate_heavy_hit()
		else:
			defender_renderer.animate_generic_hit()
		var defender_bbcode_damage := combat.bbcode_character_name(defender_battler)
		dialogue.text += "\n%s受到%s伤害" % [defender_bbcode_damage, get_damage()]
		if not heavy_suffix.is_empty():
			dialogue.text += "\n%s" % heavy_suffix
		await dialogue.pressed

	var dodge = func() -> void:
		defender_sm.action_points.value = maxf(
			0.0,
			defender_sm.action_points.value - dodge_cost,
		)
		if randf() < dodge_chance:
			Engine.time_scale = 1
			defender_renderer.animate_generic_dodge()
			dialogue.text += "\n%s轻巧地闪开了" % combat.bbcode_character_name(defender_battler)
			AudioManager.play_dodge()
			await dialogue.pressed
		else:
			var defender_bbcode_dodge := combat.bbcode_character_name(defender_battler)
			dialogue.text += "\n%s尝试闪避但是失败了" % defender_bbcode_dodge
			await deliver_damage.call()

	if combat.characters[defender_battler] == 0:
		var execution_text = get_execution_text(from_body, to_body)
		var menu = Dialogues.create_menu_dialogue()
		menu.title = execution_text
		var busy_name := (
			(defender_sm.current_state as CharacterStateMachineActionState).action_name
			if defender_in_action
			else &""
		)
		if defender_in_action and busy_name.is_empty():
			busy_name = &"动作"
		var dodge_insufficient_action_points: bool = defender_sm.action_points.value < dodge_cost
		var dodge_disabled: bool = cannot_dodge or dodge_insufficient_action_points
		var dodge_desc := "成功率%d%% 消耗行动力:%s" % [int(dodge_chance * 100), dodge_cost]
		if cannot_dodge:
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
			await deliver_damage.call()
	else:
		if cannot_dodge or defender_battler.state_machine.action_points.value < dodge_cost:
			await deliver_damage.call()
		else:
			await dodge.call()
