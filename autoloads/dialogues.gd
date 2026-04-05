extends Node

var _generic_dialogue: GenericDialogue

func show_dialogue(text: String, ...options) -> int:
	var arguments: Array = [text]
	arguments.append_array(options)
	return await _generic_dialogue.callv(&"show_message", arguments)

func show_menu(
	title: String,
	options: Array[MenuItemData],
	on_pressed: Callable) -> MenuDialogue:
	var scene:PackedScene = ResourceLoader.load("res://dialogues/menu_dialogue.tscn")
	var dialogue:MenuDialogue = scene.instantiate()
	dialogue.title = title
	dialogue.options = options
	dialogue.pressed.connect(on_pressed)
	add_child(dialogue)
	return dialogue

func _ready() -> void:
	var scene:PackedScene = ResourceLoader.load("res://dialogues/generic_dialogue.tscn")
	_generic_dialogue = scene.instantiate()
	add_child(_generic_dialogue)
