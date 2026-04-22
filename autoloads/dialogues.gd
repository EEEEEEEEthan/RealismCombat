extends Node


const _MENU_DIALOGUE_SCENE: PackedScene = preload("res://dialogues/menu_dialogue.tscn")
const _GENERIC_DIALOGUE_SCENE: PackedScene = preload("res://dialogues/generic_dialogue.tscn")


func create_menu_dialogue() -> MenuDialogue:
	var dialogue: MenuDialogue = _MENU_DIALOGUE_SCENE.instantiate()
	add_child(dialogue)
	return dialogue


func create_generic_dialogue() -> GenericDialogue:
	var dialogue: GenericDialogue = _GENERIC_DIALOGUE_SCENE.instantiate()
	add_child(dialogue)
	return dialogue
