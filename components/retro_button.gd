@tool
extends Button
class_name RetroButton

static var _atlas: ImageTexture:
	get:
		if not _atlas:
			var bytes = Marshalls.base64_to_raw("iVBORw0KGgoAAAANSUhEUgAAAAoAAAAICAYAAADA+m62AAAAAXNSR0IArs4c6QAAAD5JREFUGJWFj0EOACAIwzbj/788DwSjQ2KPrINABMIN0QwkhUvyWSjiDqww/EQiCWe5FZ3ZBXa6it9n3PcFC5f2EBLP+MZVAAAAAElFTkSuQmCC")
			var image = Image.new()
			var error = image.load_png_from_buffer(bytes)
			_atlas = ImageTexture.new()
			if error == OK:
				_atlas.set_image(image)
			else:
				push_error(error)
		return _atlas

static var _icon: AtlasTexture:
	get:
		if not _icon:
			_icon = AtlasTexture.new()
			_icon.atlas = _atlas
			_icon.region = Rect2(1, 0, 9, 8)
		return _icon

static var _icon_pressed: AtlasTexture:
	get:
		if not _icon_pressed:
			_icon_pressed = AtlasTexture.new()
			_icon_pressed.atlas = _atlas
			_icon_pressed.region = Rect2(0, 0, 9, 8)
		return _icon_pressed

static var _icon_empty: Texture2D:
	get:
		if not _icon_empty:
			var image = Image.create_empty(9, 8, false, Image.FORMAT_RGBA8)
			image.fill(Color.TRANSPARENT)
			_icon_empty = ImageTexture.create_from_image(image)
		return _icon_empty

func _ready() -> void:
	connect(&"mouse_entered", _on_mouse_entered)
	connect(&"focus_entered", _on_focus_entered)
	connect(&"focus_exited", _on_focus_exited)
	connect(&"pressed", _on_pressed)
	connect(&"button_down", _on_button_down)
	connect(&"button_up", _on_button_up)
	icon = _icon_empty

func _on_mouse_entered() -> void:
	grab_focus()

func _on_focus_entered() -> void:
	_update_icon()

func _on_focus_exited() -> void:
	_update_icon()

func _on_pressed() -> void:
	_update_icon()

func _on_button_down() -> void:
	grab_focus()
	_update_icon()

func _on_button_up() -> void:
	_update_icon()

func _update_icon() -> void:
	if not has_focus():
		icon = _icon_empty
		return
	if button_pressed:
		icon = _icon_pressed
		return
	icon = _icon
