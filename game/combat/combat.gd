extends Node
class_name Combat

const _menu_item_data_script: GDScript = preload("res://dialogues/menu_item_data.gd")

var characters: Dictionary[Character, int] = {}

func add_character(character_data: Character, side: int) -> void:
	var character: CharacterRenderer = %CharacterPlaceHolder.create_instance()
	character.bind(character_data)
	if side == 0:
		character.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character.layout_direction = Control.LAYOUT_DIRECTION_RTL
	characters[character_data] = side

func run() -> void:
	while true:
		%Timer.start(0.3)
		await %Timer.timeout
		for chr: Character in characters.keys():
			if not chr.alive:
				continue
			chr.action_points.value += chr.speed
			if chr.action_points.value >= chr.action_points.max_value:
				var side = characters[chr]
				if side == 0:
					await player_input(chr)
				else:
					await ai_input(chr)

func player_input(character: Character) -> void:
	pass

func ai_input(character: Character) -> void:
	pass
