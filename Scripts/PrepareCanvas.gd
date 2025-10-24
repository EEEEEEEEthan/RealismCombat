extends Node

class_name PrepareCanvas

@onready var game: Game = get_parent()
@onready var button_next_battle: Button = $ButtonNextBattle

func _ready() -> void:
	button_next_battle.pressed.connect(_on_button_pressed)

func _on_button_pressed() -> void:
	game.command_handler.handle(StartNextCombat.NAME)
