extends PanelContainer
class_name GenericDialogue


signal pressed(index: int)


var _active: bool = true
var _title: String = ""
var _text: String = ""
var _options: Array = []


var active: bool:
	get:
		return _active
	set(value):
		if _active == value:
			return
		_active = value
		if is_node_ready():
			_refresh_active()


var title: String:
	get:
		return _title
	set(value):
		_title = value
		if is_node_ready():
			_refresh_title()


var text: String:
	get:
		return _text
	set(value):
		_text = value
		if is_node_ready():
			_refresh_text()


var options: Array:
	get:
		return _options
	set(value):
		_options = value
		if is_node_ready():
			_refresh_options()


var _typewriter_duration: float = 0.02
var _presentation_token: int = 0
var _last_selected_index: int = -1

@onready var continue_button: TextureButton = %Continue
@onready var options_container: HBoxContainer = %Options
@onready var rich_text_label: RichTextLabel = %RichTextLabel
@onready var typewriter_timer: Timer = %TypewriterTimer

func _ready() -> void:
	continue_button.pressed.connect(_on_continue_button_pressed)
	continue_button.focus_entered.connect(_on_continue_focus_entered)
	_refresh_title()
	_refresh_options()
	_refresh_text()
	_refresh_active()


func _notification(what: int) -> void:
	if what != NOTIFICATION_VISIBILITY_CHANGED:
		return
	if not is_node_ready():
		return
	if visible and active:
		_refresh_active()

func _on_continue_button_pressed() -> void:
	pressed.emit(0)


func _refresh_title() -> void:
	pass


func _refresh_text() -> void:
	rich_text_label.text = text
	_restart_presentation()


func _refresh_options() -> void:
	_rebuild_option_buttons()
	_restart_presentation()


func _refresh_active() -> void:
	_refresh_button_states()
	if active and visible:
		_restore_focus.call_deferred()


func _run_presentation(presentation_token: int) -> void:
	await _play_typewriter(presentation_token)
	if presentation_token != _presentation_token:
		return
	if options.is_empty():
		continue_button.visible = true
	else:
		for option_index in range(options.size()):
			_get_option_button(option_index).visible = true
	_refresh_button_states()
	if active and visible:
		_restore_focus.call_deferred()

func _on_option_button_pressed(option_index: int) -> void:
	pressed.emit(option_index)


func _play_typewriter(presentation_token: int) -> void:
	var visible_character_count: int = rich_text_label.get_total_character_count()
	if visible_character_count <= 0:
		rich_text_label.visible_characters = -1
		return
	for visible_character_index in range(visible_character_count):
		if presentation_token != _presentation_token:
			return
		rich_text_label.visible_characters = visible_character_index + 1
		typewriter_timer.start(_typewriter_duration)
		await typewriter_timer.timeout

func _clear_option_buttons() -> void:
	for child_node in options_container.get_children():
		if child_node == continue_button:
			continue
		child_node.free()


func _rebuild_option_buttons() -> void:
	_clear_option_buttons()
	for option_index in range(options.size()):
		var option_button := RetroButton.new()
		var option: MenuItemData = options[option_index]
		option_button.text = option.text
		option_button.disabled = option.disabled
		option_button.focus_entered.connect(_on_option_focus_entered.bind(option_index))
		option_button.pressed.connect(_on_option_button_pressed.bind(option_index))
		options_container.add_child(option_button)


func _restart_presentation() -> void:
	if not is_node_ready():
		return
	_presentation_token += 1
	var presentation_token := _presentation_token
	rich_text_label.visible_characters = 0
	continue_button.visible = false
	for option_index in range(options.size()):
		_get_option_button(option_index).visible = false
	_refresh_button_states()
	_run_presentation.call_deferred(presentation_token)


func _refresh_button_states() -> void:
	var continue_can_focus := active and continue_button.visible
	continue_button.mouse_filter = (
		Control.MOUSE_FILTER_STOP
		if continue_can_focus
		else Control.MOUSE_FILTER_IGNORE
	)
	continue_button.focus_mode = Control.FOCUS_ALL if continue_can_focus else Control.FOCUS_NONE
	for option_index in range(options.size()):
		var option_button := _get_option_button(option_index)
		var option: MenuItemData = options[option_index]
		var can_focus: bool = active and option_button.visible and not option.text.is_empty()
		option_button.mouse_filter = Control.MOUSE_FILTER_STOP if can_focus else Control.MOUSE_FILTER_IGNORE
		option_button.focus_mode = Control.FOCUS_ALL if can_focus else Control.FOCUS_NONE


func _restore_focus() -> void:
	if not active or not visible:
		return
	if options.is_empty():
		if continue_button.visible:
			continue_button.grab_focus()
		return
	var option_index := _get_preferred_option_index()
	if option_index < 0:
		return
	_last_selected_index = option_index
	_get_option_button(option_index).grab_focus()


func _get_preferred_option_index() -> int:
	if _can_focus_option(_last_selected_index):
		return _last_selected_index
	for option_index in range(options.size()):
		if _can_focus_option(option_index):
			return option_index
	return -1


func _can_focus_option(option_index: int) -> bool:
	if option_index < 0 or option_index >= options.size():
		return false
	if options[option_index].text.is_empty():
		return false
	return _get_option_button(option_index).visible


func _get_option_button(option_index: int) -> RetroButton:
	return options_container.get_child(option_index + 1) as RetroButton


func _on_continue_focus_entered() -> void:
	_last_selected_index = -1


func _on_option_focus_entered(option_index: int) -> void:
	_last_selected_index = option_index
