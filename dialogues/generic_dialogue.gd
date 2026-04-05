extends PanelContainer
class_name GenericDialogue

signal _choice_resolved(option_index: int)

var _busy: bool = false
var _typewriter_duration: float = 0.02

func _ready() -> void:
	visible = false
	%Continue.pressed.connect(_on_continue_button_pressed)

func _on_continue_button_pressed() -> void:
	_choice_resolved.emit(0)

func show_message(message_text: String, ...option_texts) -> int:
	assert(not _busy)
	_busy = true
	var option_labels: PackedStringArray = PackedStringArray()
	for raw_option_text in option_texts:
		option_labels.append(str(raw_option_text))
	var use_continue_only: bool = option_labels.is_empty()
	%RichTextLabel.text = message_text
	%RichTextLabel.visible_characters = 0
	_clear_option_buttons()
	%Continue.visible = false
	if not use_continue_only:
		for option_index in range(option_labels.size()):
			var option_button: RetroButton = RetroButton.new()
			option_button.text = option_labels[option_index]
			var captured_index: int = option_index
			option_button.pressed.connect(func(): _on_option_button_pressed(captured_index))
			%Options.add_child(option_button)
			option_button.visible = false
	visible = true
	await _play_typewriter()
	if use_continue_only:
		%Continue.visible = true
	else:
		for child_node in %Options.get_children():
			if child_node == %Continue:
				continue
			child_node.visible = true
	if use_continue_only:
		%Continue.call_deferred(&"grab_focus")
	else:
		for child_node in %Options.get_children():
			if child_node == %Continue:
				continue
			var first_option: Control = child_node as Control
			first_option.call_deferred(&"grab_focus")
			break
	var chosen: int = await _choice_resolved
	_busy = false
	visible = false
	return chosen

func _on_option_button_pressed(option_index: int) -> void:
	_choice_resolved.emit(option_index)

func _play_typewriter() -> void:
	var visible_character_count: int = %RichTextLabel.get_total_character_count()
	if visible_character_count <= 0:
		%RichTextLabel.visible_characters = -1
		return
	for visible_character_index in range(visible_character_count):
		%RichTextLabel.visible_characters = visible_character_index + 1
		%TypewriterTimer.start(_typewriter_duration)
		await %TypewriterTimer.timeout

func _clear_option_buttons() -> void:
	for child_node in %Options.get_children():
		if child_node == %Continue:
			continue
		child_node.free()
