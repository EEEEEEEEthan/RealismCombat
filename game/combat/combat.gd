extends Node
class_name Combat

const _menu_item_data_script: GDScript = preload("res://dialogues/menu_item_data.gd")

func add_character(character_data: Character, side: int) -> void:
	var character: CharacterRenderer = %CharacterPlaceHolder.create_instance()
	character.setup(character_data)
	if side == 0:
		character.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character.layout_direction = Control.LAYOUT_DIRECTION_RTL

func run() -> void:
	pass
	#var menu = Dialogues.create_menu()
	#menu.title = "战斗"
	#
