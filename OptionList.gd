@tool
extends MarginContainer
class_name OptionList

static var _default_indicator_icon: Texture2D = ThemeDB.get_default_theme().get_icon("arrow_collapsed", "Tree")
static var _transparent_icon: ImageTexture = null
static var _empty_style: StyleBoxEmpty = StyleBoxEmpty.new()

@export_range(3, 64) var viewport_count: int = 8:
	set(value):
		viewport_count = value
		_try_update(_update_viewport)

@export var _options: PackedStringArray:
	set(value):
		if len(value) > 64:
			value = value.slice(0, 64)
		_options = value
		_try_update(_update_viewport)

@export var _disabled_mask: int:
	set(value):
		_disabled_mask = value
		_try_update(_update_viewport)

@export_group("Theme Overrides")

@export_subgroup("colors")

@export var override_font_color: bool = false:
	get:
		return has_theme_color_override("font_color")
	set(v):
		if v:
			add_theme_color_override("font_color", font_color)
		else:
			remove_theme_color_override("font_color")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font_color: Color = Color(1, 1, 1, 1):
	get:
		if not has_theme_color_override("font_color"):
			return get_theme_color("font_color", "Button")
		if has_theme_color_override("font_color"):
			return get_theme_color("font_color")
		return get_theme_color("font_color", "OptionList")
	set(v):
		if override_font_color:
			add_theme_color_override("font_color", v)
		_try_update(_update_all_button_themes)

@export var override_focus_color: bool = false:
	get:
		return has_theme_color_override("font_focus_color")
	set(v):
		if v:
			add_theme_color_override("font_focus_color", font_focus_color)
		else:
			remove_theme_color_override("font_focus_color")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font_focus_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color("font_focus_color"):
			return get_theme_color("font_focus_color", "Button")
		if has_theme_color_override("font_focus_color"):
			return get_theme_color("font_focus_color")
		return get_theme_color("font_focus_color", "OptionList")
	set(v):
		if override_focus_color:
			add_theme_color_override("font_focus_color", v)
		_try_update(_update_all_button_themes)

@export var override_disabled_color: bool = false:
	get:
		return has_theme_color_override("font_disabled_color")
	set(v):
		if v:
			add_theme_color_override("font_disabled_color", font_disabled_color)
		else:
			remove_theme_color_override("font_disabled_color")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font_disabled_color: Color = Color(0, 0, 0, 0):
	get:
		if not has_theme_color("font_disabled_color"):
			return get_theme_color("font_disabled_color", "Button")
		if has_theme_color_override("font_disabled_color"):
			return get_theme_color("font_disabled_color")
		return get_theme_color("font_disabled_color", "OptionList")
	set(v):
		if override_disabled_color:
			add_theme_color_override("font_disabled_color", v)
		_try_update(_update_all_button_themes)

@export_subgroup("fonts")

@export var override_font: bool = false:
	get:
		return has_theme_font_override("font")
	set(v):
		if v:
			var resolved_font := font
			if resolved_font:
				add_theme_font_override("font", resolved_font)
			else:
				remove_theme_font_override("font")
		else:
			remove_theme_font_override("font")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export var font: Font = null:
	get:
		if not has_theme_font("font"):
			return get_theme_font("font", "Button")
		if has_theme_font_override("font"):
			return get_theme_font("font")
		return get_theme_font("font", "OptionList")
	set(v):
		if override_font:
			if v:
				add_theme_font_override("font", v)
			else:
				remove_theme_font_override("font")
		_try_update(_update_all_button_themes)

@export var override_font_size: bool = false:
	get:
		return has_theme_font_size_override("font_size")
	set(v):
		if v:
			add_theme_font_size_override("font_size", font_size)
		else:
			remove_theme_font_size_override("font_size")
		notify_property_list_changed()
		_try_update(_update_all_button_themes)

@export_range(0, 128) var font_size: int = 12:
	get:
		if not has_theme_font_size("font_size"):
			return get_theme_font_size("font_size", "Button")
		if has_theme_font_size_override("font_size"):
			return get_theme_font_size("font_size")
		return get_theme_font_size("font_size", "OptionList")
	set(v):
		if override_font_size:
			add_theme_font_size_override("font_size", v)
		_try_update(_update_all_button_themes)

@export_subgroup("icons")

@export var _override_indexer_icon: bool = false:
	get:
		return has_theme_icon_override("indexer_icon")
	set(v):
		if v:
			add_theme_icon_override("indexer_icon", _indexer_icon if _indexer_icon else _default_indicator_icon)
		else:
			remove_theme_icon_override("indexer_icon")
		notify_property_list_changed()
		_try_update(_update_theme)

@export var _indexer_icon: Texture2D = null:
	get:
		if not has_theme_icon("indexer_icon"):
			return _default_indicator_icon
		if has_theme_icon_override("indexer_icon"):
			return get_theme_icon("indexer_icon")
		var from_option_list := get_theme_icon("indexer_icon", "OptionList")
		return from_option_list if from_option_list else _default_indicator_icon
	set(v):
		if _override_indexer_icon:
			add_theme_icon_override("indexer_icon", v if v else _default_indicator_icon)
		notify_property_list_changed()
		_try_update(_update_theme)

signal option_focused(option_index: int)
signal option_selected(option_index: int)

var _options_container: VBoxContainer

var _viewport_start_index: int

func _validate_property(property: Dictionary) -> void:
	if property.name == "_override_indexer_icon":
		property.usage = PROPERTY_USAGE_EDITOR
	elif property.name == "_indexer_icon":
		if _override_indexer_icon:
			property.usage = PROPERTY_USAGE_EDITOR
			property.hint = PROPERTY_HINT_RESOURCE_TYPE
			property.hint_string = "Texture2D"
		else:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_color":
		if not override_font_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_focus_color":
		if not override_focus_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_disabled_color":
		if not override_disabled_color:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font":
		if not override_font:
			property.usage = PROPERTY_USAGE_NO_EDITOR
	elif property.name == "font_size":
		if not override_font_size:
			property.usage = PROPERTY_USAGE_NO_EDITOR

func _notification(notification_type: int) -> void:
	if notification_type == NOTIFICATION_THEME_CHANGED and is_node_ready():
		_update_theme()

func _ready() -> void:
	_options_container = VBoxContainer.new()
	_options_container.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_options_container)
	_try_update(_update_theme)
	_try_update(_update_viewport)

func _try_update(update_function: Callable) -> void:
	if is_node_ready():
		update_function.call()

func _update_theme() -> void:
	if _indexer_icon:
		var icon_size = max(_indexer_icon.get_width(), _indexer_icon.get_height())
		if _transparent_icon == null or _transparent_icon.get_width() != icon_size:
			var image = Image.create(icon_size, icon_size, false, Image.FORMAT_RGBA8)
			image.fill(Color.TRANSPARENT)
			_transparent_icon = ImageTexture.create_from_image(image)
	_update_all_button_icons()
	_update_all_button_themes()

func _update_viewport() -> void:
	var node_count = _options_container.get_child_count()
	for index in range(node_count - viewport_count):
		_options_container.get_child(node_count - index - 1).queue_free()
	for index in range(viewport_count - node_count):
		_options_container.add_child(_create_button())
	var visible_count = min(viewport_count, len(_options))
	for index in range(visible_count):
		var button = _options_container.get_child(index) as Button
		if index == 0 and _viewport_start_index > 0:
			button.text = "...+" + str(_viewport_start_index + 1)
		elif viewport_count - 1 == index and _viewport_start_index + viewport_count < len(_options):
			button.text = "...+" + str(len(_options) - (_viewport_start_index + viewport_count) + 1)
		elif index + _viewport_start_index < len(_options):
			button.text = _options[index + _viewport_start_index]
		else:
			button.text = ""
		button.disabled = (((1 << index) & _disabled_mask) != 0)
	for index in range(visible_count, viewport_count):
		(_options_container.get_child(index) as Button).text = ""
	_update_all_button_icons()
	_update_all_button_themes()

func _on_button_focused(button: Button) -> void:
	button.icon = _indexer_icon
	var button_index = button.get_index()
	if button_index == 0 and _viewport_start_index > 0:
		_viewport_start_index -= 1
		button.get_parent().get_child(1).grab_focus()
		call_deferred("_update_viewport")
	elif button_index == viewport_count - 1 and _viewport_start_index + viewport_count < len(_options):
		_viewport_start_index += 1
		button.get_parent().get_child(button_index - 1).grab_focus()
		call_deferred("_update_viewport")
	else:
		var option_index = _calculate_option_index(button_index)
		if option_index >= 0 and option_index < len(_options):
			option_focused.emit(option_index)

func _on_button_focus_lost(button: Button) -> void:
	button.icon = _transparent_icon

func _on_button_mouse_entered(button: Button) -> void:
	button.grab_focus()

func _on_button_pressed(button: Button) -> void:
	var button_index = button.get_index()
	var option_index = _calculate_option_index(button_index)
	if not button.disabled and option_index >= 0 and option_index < len(_options) and not button.text.begins_with("..."):
		option_selected.emit(option_index)

func _create_button() -> Button:
	var button = Button.new()
	button.focus_entered.connect(_on_button_focused.bind(button))
	button.focus_exited.connect(_on_button_focus_lost.bind(button))
	button.mouse_entered.connect(_on_button_mouse_entered.bind(button))
	button.pressed.connect(_on_button_pressed.bind(button))
	button["theme_override_styles/focus"] = _empty_style
	button.flat = true
	button.alignment = HORIZONTAL_ALIGNMENT_LEFT
	if _transparent_icon:
		button.icon = _transparent_icon
	return button

func _calculate_option_index(button_index: int) -> int:
	return button_index + _viewport_start_index

func _update_all_button_icons() -> void:
	var node_count = _options_container.get_child_count()
	for i in range(node_count):
		var button = _options_container.get_child(i) as Button
		if button:
			if button.has_focus():
				button.icon = _indexer_icon
			else:
				button.icon = _transparent_icon

func _update_all_button_themes() -> void:
	var node_count = _options_container.get_child_count()
	for i in range(node_count):
		var button := _options_container.get_child(i) as Button
		if not button:
			continue

		if font:
			button.add_theme_font_override("font", font)
		else:
			button.remove_theme_font_override("font")

		button.add_theme_font_size_override("font_size", font_size)

		button.add_theme_color_override("font_color", font_color)
		button.add_theme_color_override("font_focus_color", font_focus_color)
		button.add_theme_color_override("font_disabled_color", font_disabled_color)
