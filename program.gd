extends Node
class_name Program

func create_new_game() -> Game:
	var game = %Game.create_instance()
	print(game)
	return game
