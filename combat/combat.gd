extends Node

func add_character(character_data: Character, side: int) -> void:
	%CharacterPlaceHolder.create_instance()

func _ready() -> void:
	var chr = Character.new()
	add_character(chr, 0)
