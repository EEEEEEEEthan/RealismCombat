extends Node
class_name Combat

const _menu_item_data_script: GDScript = preload("res://dialogues/menu_item_data.gd")

var characters: Dictionary[Character, int] = {}

func add_character(character: Character, side: int) -> void:
	var chr: CharacterRenderer = %CharacterPlaceHolder.create_instance()
	character.on_enter_combat()
	chr.bind(character)
	if side == 0:
		chr.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		chr.layout_direction = Control.LAYOUT_DIRECTION_RTL
	characters[character] = side
	await tree_exited
	character.on_exit_combat()

func run() -> void:
	while true:
		for chr: Character in characters.keys():
			if not chr.alive:
				continue
			await chr.state_machine.new_tick()
		%Timer.start(0.3)
		await %Timer.timeout
