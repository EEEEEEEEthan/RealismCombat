extends Node
class_name Program

func create_new_game() -> Game:
	var game: Game = %Game.create_instance()
	game.new_game()
	return game
