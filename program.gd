extends Node
class_name Program

func create_new_game() -> Game:
	return %Game.create_instance()

func show_main_menu() -> MainMenu:
	return %MainMenu.create_instance()

func _ready() -> void:
	show_main_menu()
