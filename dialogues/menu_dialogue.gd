extends PanelContainer
class_name MenuDialogue


signal pressed(index: int)


const _META_MENU_BUTTON_WIRED: StringName = &"menu_dialogue_button_wired"


static func create(tree: SceneTree) -> MenuDialogue:
	var scene:PackedScene = ResourceLoader.load("res://dialogues/menu_dialogue.tscn")
	var dialogue:MenuDialogue = scene.instantiate()
	tree.root.add_child(dialogue)
	return dialogue


@export var title: String:
	set(v):
		title = v
		_update_title()


@export var options: Array[MenuItemData]:
	set(v):
		options = v
		_update_menu()


func _ready() -> void:
	_update_title()
	for button:RetroButton in %RetroScrollContainer.get_children():
		_connect_button(button)
	_update_menu()


func _update_title() -> void:
	if not is_node_ready(): return
	%Title.text = title


func _update_menu() -> void:
	if not is_node_ready(): return
	var container := %RetroScrollContainer
	var child_count = container.get_child_count()
	var length = len(options)
	if child_count < length:
		for i in length - child_count:
			var button = RetroButton.new()
			_connect_button(button)
			container.add_child(button)
	elif child_count > length:
		for i in child_count - length:
			container.get_child(child_count - i - 1).queue_free()
	for i in length:
		var button: RetroButton = container.get_child(i)
		button.alignment = HORIZONTAL_ALIGNMENT_LEFT
		button.text = options[i].text
		button.disabled = options[i].disabled
		if options[i].text:
			button.mouse_filter = Control.MOUSE_FILTER_STOP
			button.focus_mode = Control.FOCUS_ALL
		else:
			button.mouse_filter = Control.MOUSE_FILTER_IGNORE
			button.focus_mode = Control.FOCUS_NONE


func _connect_button(button: RetroButton) -> void:
	button.focus_entered.connect(_on_focus_button.bind(button))
	button.pressed.connect(_on_press_button.bind(button))


func _on_focus_button(button: RetroButton) -> void:
	%RichTextLabel.text = options[button.get_index()].description


func _on_press_button(button: RetroButton) -> void:
	pressed.emit(button.get_index())


func _on_visibility_changed() -> void:
	if visible:
		_grab_focus.call_deferred()

func _grab_focus() -> void:
	var container = %RetroScrollContainer
	if not container:
		return
	if container.last_selected > 0:
		container.get_child(container.last_selected).grab_focus()
	elif container.get_child_count() > 0:
		container.get_child(0).grab_focus()
