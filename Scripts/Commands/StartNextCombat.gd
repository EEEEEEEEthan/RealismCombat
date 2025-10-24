extends RefCounted

class_name StartNextCombat

const NAME: String = "game.start_next_combat"

var _game: Game

func _init(game: Game) -> void:
	_game = game

func execute() -> void:
	_game.log("战斗开始了!")
