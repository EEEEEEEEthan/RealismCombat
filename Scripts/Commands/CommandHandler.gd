extends Node

class_name CommandHandler

@onready var game: Game = $".."

func handle(command: String) -> String:
	if command == "game.check_status":
		return "这个功能还没实现";
	if command == "game.start_next_combat":
		return StartNextCombat.new().execute()
	return "unknown command: " + command
