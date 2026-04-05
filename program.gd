extends Node
class_name Program

func create_new_game() -> Game:
	return %Game.create_instance()
