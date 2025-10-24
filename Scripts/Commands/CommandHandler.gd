extends RefCounted

class_name CommandHandler

var _game: Game
var logs: Array[String] = []

func _init(game: Game) -> void:
	_game = game

func handle(command: String) -> String:
	return "以后再实现"
	logs.clear()
	_game.on_log.connect(_on_log)
	if command == "game.check_status":
		_game.log("这个功能还没实现")
	elif command == StartNextCombat.NAME:
		await StartNextCombat.new().execute()
	else:
		_game.log("unknown command: " + command)
	_game.on_log.disconnect(_on_log)
	return "\n".join(logs)

func _on_log(msg: String) -> void:
	logs.append(msg)
