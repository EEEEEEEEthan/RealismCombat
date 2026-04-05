extends Node
class_name Combat

func add_character(character_data: Character, side: int) -> void:
	var character: CharacterRenderer = %CharacterPlaceHolder.create_instance()
	character.setup(character_data)
	if side == 0:
		character.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character.layout_direction = Control.LAYOUT_DIRECTION_RTL

	#var dialogue = GenericDialogue.create(get_tree())
	#await dialogue.show_message("战斗开始了!")

	#await GenericDialogue.show_message(get_tree(), "战斗开始了!")
