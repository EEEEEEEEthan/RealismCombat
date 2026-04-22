extends Node
class_name Program

func begin_new_game(slot_index: int) -> Game:
	var game: Game = %Game.create_instance()
	game.path = SaveSlots.path_for_slot(slot_index)
	game.new_game()
	return game


func begin_loaded_game(slot_index: int) -> Game:
	var game: Game = %Game.create_instance()
	game.load_game(SaveSlots.path_for_slot(slot_index))
	return game
