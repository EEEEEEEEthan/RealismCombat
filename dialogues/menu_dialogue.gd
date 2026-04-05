extends PanelContainer
class_name MenuDialogue

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
			button.focus_entered.connect(_on_focus_button.bind(button))
			button.pressed.connect(_on_press_button.bind(button))
			button.alignment = HORIZONTAL_ALIGNMENT_LEFT
			container.add_child(button)
	elif child_count > length:
		for i in child_count - length:
			container.get_child(child_count - i - 1).queue_free()
	for i in length:
		var button: RetroButton = container.get_child(i)
		button.text = options[i].text
		button.disabled = options[i].disabled

func _on_focus_button(button: RetroButton) -> void:
	%RichTextLabel.text = options[button.get_index()].description

func _on_press_button(button: RetroButton) -> void:
	var callback: Callable = options[button.get_index()].pressed
	if callback.is_valid():
		callback.call()
