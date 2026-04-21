extends RefCounted
class_name CharacterStateMachineIdleState

var machine: CharacterStateMachine

func _init(p_machine: CharacterStateMachine) -> void:
	machine = p_machine

func new_tick() -> void:
	var action_points := machine.action_points
	action_points.value = minf(
		action_points.value + machine.action_points_per_tick,
		action_points.max_value,
	)
	if action_points.value >= action_points.max_value:
		AudioManager.play_turn_begin()
		var character_renderer := machine.character_renderer
		character_renderer.centered = true
		character_renderer.expanded = true
		var parameter := await player_turn_choose_from_body_part() if machine.is_player else ai()
		if not parameter:
			character_renderer.expanded = false
			character_renderer.centered = false
			return
		await machine._set_action(parameter.action, parameter.from, parameter.to)

func player_turn_choose_from_body_part() -> ActionParameter:
	var dialogue := Dialogues.create_generic_dialogue()
	dialogue.text = "%s的回合!" % machine.character.character_name
	await dialogue.pressed
	dialogue.queue_free()
	var menu = Dialogues.create_menu_dialogue()
	var options: Array[MenuItemData] = [
		MenuItemData.new(&"头部..", false, &"头部状态"),
		MenuItemData.new(&"身体..", false, &"身体状态"),
		MenuItemData.new(&"右手..", false, &"右手状态"),
		MenuItemData.new(&"左手..", false, &"左手状态"),
		MenuItemData.new(&"右腿..", false, &"右腿状态"),
		MenuItemData.new(&"左腿..", false, &"左腿状态"),
	]
	menu.title = "%s的回合" % machine.character.character_name
	menu.options = options
	while true:
		var choice = await menu.pressed
		var from_body := machine.character.all_body_parts[choice]
		var parameter = await player_turn_choose_action(from_body)
		if parameter:
			menu.queue_free()
			return parameter
	return null

func player_turn_choose_action(from_body: BodyPart) -> ActionParameter:
	var menu = Dialogues.create_menu_dialogue()
	menu.title = "%s的回合>%s" % [machine.character.character_name, from_body.part_name()]
	var options: Array[MenuItemData]
	var action_list: Array[Action]
	for action in machine.character.actions:
		if not action.static_valid_from_body(from_body).success:
			continue
		var outcome = action.static_valid_to_body(from_body)
		var data = MenuItemData.new("%s.." % action.get_name())
		if not outcome.success:
			data.disabled = true
			data.description = outcome.error_message
		else:
			data.disabled = false
			data.description = action.get_description()
		options.append(data)
		action_list.append(action)
	options.append(MenuItemData.new(&"返回", false, &"返回上一级"))
	menu.options = options
	while true:
		var choice = await menu.pressed
		if choice >= len(action_list):
			break
		var action = action_list[choice]
		var parameter = await player_turn_choose_target(action, from_body)
		if parameter:
			menu.queue_free()
			return parameter
	menu.queue_free()
	return null


func player_turn_choose_target(action: Action, from_body: BodyPart) -> ActionParameter:
	var menu = Dialogues.create_menu_dialogue()
	menu.title = "%s的回合>%s>%s" % [
		machine.character.character_name,
		from_body.part_name(),
		action.get_name(),
	]
	var options: Array[MenuItemData]
	var characters: Array[Character]
	for chr in machine.combat.characters:
		if not action.static_valid_to_character(chr).success:
			continue
		var data = MenuItemData.new("%s.." % chr.character_name)
		var outcome = action.dynamic_valid_to_character(chr)
		if outcome.success:
			data.description = "选择%s为目标" % chr.character_name
		options.append(data)
		characters.append(chr)
	options.append(MenuItemData.new(&"返回", false, &"返回上一级"))
	menu.options = options
	while true:
		var choice = await menu.pressed
		if choice >= len(characters):
			break
		var chr = characters[choice]
		var parameter = await player_turn_choose_target_body(action, from_body, chr)
		if parameter:
			menu.queue_free()
			return parameter
	menu.queue_free()
	return null


func player_turn_choose_target_body(
	action: Action,
	from_body: BodyPart,
	to_character: Character,
) -> ActionParameter:
	var menu = Dialogues.create_menu_dialogue()
	menu.title = "%s的回合>%s>%s>%s" % [
		machine.character.character_name,
		from_body.part_name(),
		action.get_name(),
		to_character.character_name,
	]
	var options: Array[MenuItemData]
	var body_list: Array[BodyPart]
	for to_body in to_character.all_body_parts:
		if not action.static_valid_to_body(to_body).success:
			continue
		var data = MenuItemData.new(str(to_body.part_name()))
		var dynamic_outcome = action.dynamic_valid_to_body(to_body)
		if dynamic_outcome.success:
			data.disabled = false
			data.description = "选择%s为目标\n%s" % [to_body.part_name(), action.preview(from_body, to_body)]
		else:
			data.disabled = true
			data.description = dynamic_outcome.error_message
		options.append(data)
		body_list.append(to_body)
	options.append(MenuItemData.new(&"返回", false, &"返回上一级"))
	menu.options = options
	while true:
		var choice = await menu.pressed
		if choice >= len(body_list):
			break
		menu.queue_free()
		var parameter = ActionParameter.new()
		parameter.action = action
		parameter.from = from_body
		parameter.to = body_list[choice]
		return parameter
	menu.queue_free()
	return null

func ai() -> ActionParameter:
	var actions: Array[ActionParameter]
	var from = machine.character
	for action: Action in from.actions:
		for from_body: BodyPart in from.all_body_parts:
			if not action.valid_from_body(from_body):
				continue
			for to_character: Character in machine.combat.characters:
				if not action.valid_to_character(to_character):
					continue
				for to_body: BodyPart in to_character.all_body_parts:
					if not action.valid_to_body(to_body):
						continue
					var weight = action.get_weight(from_body, to_body)
					var parameter = ActionParameter.new()
					parameter.action = action
					parameter.from = from_body
					parameter.to = to_body
					parameter.weight = weight
					actions.append(parameter)
	if actions.is_empty():
		return
	var total_weight := 0.0
	for p in actions:
		total_weight += p.weight
	var chosen: ActionParameter = actions[0]
	if total_weight > 0.0:
		var roll := randf() * total_weight
		var acc := 0.0
		for p in actions:
			acc += p.weight
			if roll < acc:
				chosen = p
				break
	return chosen

class ActionParameter:
	var action: Action
	var from: BodyPart
	var to: BodyPart
	var weight: float
