extends PanelContainer
class_name MenuDialogue

signal pressed(option_index: int)

@export var options: Array[MenuItemData]:
	set(v):
		options = v
		_update_menu()

func _ready() -> void:
	_update_menu()

func _update_menu() -> void:
	if not is_node_ready():
		return
	var container := %RetroScrollContainer
	var child_count = container.get_child_count()
	var length = len(options)
	if child_count < length:
		for i in length - child_count:
			var button = RetroButton.new()
			button.connect(&"focus_entered", _on_focus_button.bind(button))
			button.connect(&"pressed", _on_press_button.bind(button))
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
	pressed.emit(button.get_index())
