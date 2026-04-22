@tool
extends Button
class_name RetroButton

static var _icon: AtlasTexture:
	get:
		if not _icon:
			_icon = Resources.atlas_texture_theme_right.duplicate()
			var r = _icon.region
			_icon.region = Rect2(r.position, r.size+Vector2(1,0))
		return _icon

static var _icon_pressed: AtlasTexture:
	get:
		if not _icon_pressed:
			_icon_pressed = Resources.atlas_texture_theme_right.duplicate()
			var r = _icon_pressed.region
			_icon_pressed.region = Rect2(r.position+Vector2(-1, 0), r.size+Vector2(1,0))
		return _icon_pressed

static var _icon_empty: Texture2D:
	get:
		if not _icon_empty:
			var texture_size = Resources.atlas_texture_theme_right.get_size()
			var width = texture_size.x + 1
			var height = texture_size.y + 1
			var image = Image.create(width, height, false, Image.FORMAT_RGBA8)
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
