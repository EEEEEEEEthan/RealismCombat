extends RefCounted

class_name CommandHandler

var _game: Game

func _init(game: Game) -> void:
	_game = game

func handle(command: String) -> String:
	if command == "game.check_status":
		return "这个功能还没实现";
	if command == StartNextCombat.NAME:
		return StartNextCombat.new().execute()
	return "unknown command: " + command
