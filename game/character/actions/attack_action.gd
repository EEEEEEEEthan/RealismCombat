@abstract
extends Action
class_name AttackAction

var block_cost: float = 2
var dodge_cost: float = 4

func get_dodge_chance(_from_body: BodyPart, _to_body: BodyPart) -> float:
	push_error("override me")
	return 0

func preview(from_body: BodyPart, to_body: BodyPart) -> String:
	var dodge_chance = get_dodge_chance(from_body, to_body)
	return &"闪避成功率" + str(int(dodge_chance * 100)) + &"%"

func execute(from_body: BodyPart, to_body: BodyPart) -> void:
	await super.execute(from_body, to_body)
	var combat := from_body.character.game.combat
	var attacker_renderer := combat.get_character_renderer(from_body.character)
	await attacker_renderer.animate_generic_attack()
	await _react(from_body, to_body)

func get_damage() -> Damage:
	push_error("override me")
	return Damage.new(0, 0, 0)

func _get_description() -> String:
	return super._get_description() + &"\n伤害:" + str(get_damage())

func _react(from_body: BodyPart, to_body: BodyPart) -> void:
	Engine.time_scale = 0
	var combat := to_body.character.game.combat
	var dodge_chance = get_dodge_chance(from_body, to_body)
	var defender_renderer := combat.get_character_renderer(to_body.character)

	var deliver_damage = func(show_dialogue: bool) -> void:
		await combat.get_tree().create_timer(0.3, true, false, true).timeout
		Engine.time_scale = 1
		var damage_total = get_damage().sum
		to_body.hp.value -= damage_total
		AudioManager.play_hit()
		defender_renderer.animate_generic_hit()
		if show_dialogue:
			var menu = Dialogues.create_generic_dialogue()
			menu.text = &"造成伤害:" + str(get_damage())
			await menu.pressed
			menu.queue_free()

	var dodge = func() -> void:
		var menu = Dialogues.create_generic_dialogue()
		if randf() < get_dodge_chance(from_body, to_body):
			Engine.time_scale = 1
			defender_renderer.animate_generic_dodge()
			menu.text = to_body.character.character_name + &"轻巧地闪开了"
			AudioManager.play_dodge()
			await menu.pressed
			menu.queue_free()
		else:
			await deliver_damage.call(false)
			menu.text = to_body.character.character_name + &"尝试闪避但是失败了.造成伤害:" + str(get_damage())
			await menu.pressed
			menu.queue_free()

	if combat.characters[to_body.character] == 0:
		var execution_text = get_execution_text(from_body, to_body)
		var menu = Dialogues.create_menu_dialogue()
		menu.title = execution_text
		menu.options = [
			MenuItemData.new("闪避", false, &"成功率" + str(int(dodge_chance * 100)) + &"%"),
			MenuItemData.new("硬抗"),
		] as Array[MenuItemData]
		var option = await menu.pressed
		menu.queue_free()
		if option == 0:
			await dodge.call()
		else:
			await deliver_damage.call(true)
	else:
		await dodge.call()
