@tool
extends VBoxContainer
class_name OptionList

signal option_focused(option_index: int)
signal option_pressed(option_index: int)

var _viewport_start_index: int = 0
var _hovered_button: Button = null
var _is_scrolling_up: bool = false
var _option_data_list: Array[OptionData] = []

@onready var _hover_timer: Timer:
	get:
		if not _hover_timer:
			_hover_timer = Timer.new()
			add_child(_hover_timer)
		return _hover_timer

@export_range(3, 64) var viewport_count: int = 8:
	set(value):
		viewport_count = clampi(value, 3, 64)
		if is_node_ready():
			_sync_viewport()

@export var options: Array[OptionData]:
	set(value):
		var limited_option_data_list: Array[OptionData] = []
		var option_count = min(value.size(), 64)
		for option_index in range(option_count):
			limited_option_data_list.append(value[option_index])
		_option_data_list = limited_option_data_list
		if is_node_ready():
			_sync_viewport()
	get:
		return _option_data_list

static func new_option(option_text: String, option_disabled: bool = false) -> OptionData:
	return OptionData.new(option_text, option_disabled)

func _ready() -> void:
	_sync_viewport()

func _sync_viewport() -> void:
	_clamp_viewport_start_index()
	_sync_button_count()
	var option_count = options.size()
	var visible_count = min(viewport_count, option_count)
	for button_index in range(viewport_count):
		var option_button = get_child(button_index) as Button
		if button_index < visible_count:
			_configure_button(option_button, button_index, option_count)
		else:
			option_button.text = ""
			option_button.disabled = true

func _clamp_viewport_start_index() -> void:
	var max_start_index = maxi(options.size() - viewport_count, 0)
	_viewport_start_index = clampi(_viewport_start_index, 0, max_start_index)

func _sync_button_count() -> void:
	var child_count = get_child_count()
	while child_count > viewport_count:
		var option_button = get_child(child_count - 1)
		remove_child(option_button)
		option_button.queue_free()
		child_count -= 1
	while child_count < viewport_count:
		add_child(_create_button())
		child_count += 1

func _configure_button(option_button: Button, button_index: int, option_count: int) -> void:
	if button_index == 0 and _viewport_start_index > 0:
		option_button.text = "...+" + str(_viewport_start_index + 1)
		option_button.disabled = false
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count:
		option_button.text = "...+" + str(option_count - (_viewport_start_index + viewport_count) + 1)
		option_button.disabled = false
	elif button_index + _viewport_start_index < option_count:
		var option_data = options[button_index + _viewport_start_index]
		option_button.text = option_data.text
		option_button.disabled = option_data.disabled
	else:
		option_button.text = ""
		option_button.disabled = true

func _on_button_focused(button: Button) -> void:
	var button_index = button.get_index()
	var option_count = options.size()
	if button_index == 0 and _viewport_start_index > 0:
		_viewport_start_index -= 1
		(get_child(1) as Button).grab_focus()
		call_deferred(&"_sync_viewport")
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count:
		_viewport_start_index += 1
		(get_child(button_index - 1) as Button).grab_focus()
		call_deferred(&"_sync_viewport")
	else:
		var option_index = button_index + _viewport_start_index
		if option_index >= 0 and option_index < option_count:
			option_focused.emit(option_index)

func _on_button_mouse_entered(button: Button) -> void:
	var button_index = button.get_index()
	var option_count = options.size()
	var can_scroll_up = button_index == 0 and _viewport_start_index > 0
	var can_scroll_down = button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count
	if can_scroll_up or can_scroll_down:
		_hovered_button = button
		_is_scrolling_up = can_scroll_up
		_hover_timer.start()
		return
	button.grab_focus()
	_hovered_button = null
	_hover_timer.stop()

func _on_button_mouse_exited(button: Button) -> void:
	if _hovered_button == button:
		_hovered_button = null
		_hover_timer.stop()

func _on_hover_timer_timeout() -> void:
	if not _hovered_button or not is_instance_valid(_hovered_button):
		_hovered_button = null
		_hover_timer.stop()
		return
	var button_index = _hovered_button.get_index()
	var option_count = options.size()
	if _is_scrolling_up:
		if button_index == 0 and _viewport_start_index > 0:
			_viewport_start_index -= 1
			call_deferred(&"_sync_viewport")
			call_deferred(&"_defer_refresh_hover_after_scroll", true)
			return
		_defer_refresh_hover_after_scroll(true)
		return
	if button_index == viewport_count - 1 and _viewport_start_index + viewport_count < option_count:
		_viewport_start_index += 1
		call_deferred(&"_sync_viewport")
		call_deferred(&"_defer_refresh_hover_after_scroll", false)
		return
	_defer_refresh_hover_after_scroll(false)

func _defer_refresh_hover_after_scroll(scroll_up: bool) -> void:
	var child_count = get_child_count()
	if child_count == 0:
		return
	var edge_index = 0 if scroll_up else child_count - 1
	var edge_button = get_child(edge_index) as Button
	if edge_button.text.begins_with("..."):
		_hovered_button = edge_button
		return
	_hovered_button = null
	_hover_timer.stop()
	var start_index = 0 if scroll_up else child_count - 1
	var end_index = child_count if scroll_up else -1
	var index_step = 1 if scroll_up else -1
	for button_index in range(start_index, end_index, index_step):
		var option_button = get_child(button_index) as Button
		if option_button.text != "" and not option_button.text.begins_with("..."):
			option_button.grab_focus()
			return

func _on_button_pressed(button: Button) -> void:
	var button_index = button.get_index()
	var option_count = options.size()
	var option_index = button_index + _viewport_start_index
	if not button.disabled and option_index >= 0 and option_index < option_count and not button.text.begins_with("..."):
		option_pressed.emit(option_index)

func _create_button() -> Button:
	var option_button := CustomButton.new()
	option_button.name = "OptionButton"
	option_button.visible = true
	option_button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	option_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	option_button.focus_entered.connect(_on_button_focused.bind(option_button))
	option_button.mouse_entered.connect(_on_button_mouse_entered.bind(option_button))
	option_button.mouse_exited.connect(_on_button_mouse_exited.bind(option_button))
	option_button.pressed.connect(_on_button_pressed.bind(option_button))
	return option_button
