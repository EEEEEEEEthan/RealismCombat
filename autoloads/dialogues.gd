extends Node


const _MENU_DIALOGUE_SCENE: PackedScene = preload("res://dialogues/menu_dialogue.tscn")
const _GENERIC_DIALOGUE_SCENE: PackedScene = preload("res://dialogues/generic_dialogue.tscn")


var _dialogue_stack: Array[Control] = []


func create_menu_dialogue() -> MenuDialogue:
	var dialogue: MenuDialogue = _MENU_DIALOGUE_SCENE.instantiate()
	_push_dialogue(dialogue)
	return dialogue


func create_generic_dialogue() -> GenericDialogue:
	var dialogue: GenericDialogue = _GENERIC_DIALOGUE_SCENE.instantiate()
	_push_dialogue(dialogue)
	return dialogue


func _push_dialogue(dialogue: Control) -> Control:
	var top_dialogue: Control = _get_top_dialogue()
	if top_dialogue:
		top_dialogue.set(&"active", false)
	dialogue.visible = false
	dialogue.set(&"active", false)
	add_child(dialogue)
	_dialogue_stack.append(dialogue)
	dialogue.tree_exiting.connect(_on_dialogue_tree_exiting.bind(dialogue), CONNECT_ONE_SHOT)
	_activate_dialogue.call_deferred(dialogue)
	return dialogue


func _activate_dialogue(dialogue: Control) -> void:
	if not is_instance_valid(dialogue):
		return
	dialogue.visible = true
	dialogue.set(&"active", true)


func _on_dialogue_tree_exiting(dialogue: Control) -> void:
	var stack_index := _dialogue_stack.find(dialogue)
	if stack_index < 0:
		return
	var was_top := stack_index == _dialogue_stack.size() - 1
	_dialogue_stack.remove_at(stack_index)
	if was_top:
		_reactivate_top_dialogue.call_deferred()


func _reactivate_top_dialogue() -> void:
	var top_dialogue: Control = _get_top_dialogue()
	if top_dialogue:
		top_dialogue.set(&"active", true)


func _get_top_dialogue() -> Control:
	while not _dialogue_stack.is_empty():
		var top_dialogue: Control = _dialogue_stack[-1]
		if is_instance_valid(top_dialogue):
			return top_dialogue
		_dialogue_stack.pop_back()
	return null
