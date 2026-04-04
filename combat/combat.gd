extends Node

func add_character(character_data: Character, side: int) -> void:
	var character: CharacterRenderer = %CharacterPlaceHolder.create_instance()
	character.setup(character_data)
	if side == 0:
		character.layout_direction = Control.LAYOUT_DIRECTION_LTR
	else:
		character.layout_direction = Control.LAYOUT_DIRECTION_RTL

func _ready() -> void:
	var chr = Character.new()
	chr.character_name = "Ethan"
	add_character(chr, 0)
