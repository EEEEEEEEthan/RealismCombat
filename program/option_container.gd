@tool
extends VBoxContainer
class_name OptionContainer

var _arrow_texture: AtlasTexture:
	get:
		if not _arrow_texture:
			var image = ResourceLoader.load("uid://dkajou5ypu7d0") as Texture2D
			assert(image)
			_arrow_texture = AtlasTexture.new()
			_arrow_texture.atlas = image
			_arrow_texture.region = Rect2(21, 2, 8, 5)
		return _arrow_texture

@export_range(0, 32, 1, &"prefer_slider") var viewport_begin: int:
	get:
		var child_count = get_child_count()
		if child_count > viewport_size:
			return clamp(viewport_begin, 0, child_count - viewport_size)
		return 0
	set(value):
		viewport_begin = value
		_update_viewport()

@export_range(3, 16, 1, &"prefer_slider") var viewport_size: int = 8:
	set(value):
		viewport_size = value
		_update_viewport()

var viewport_end: int:
	get:
		return viewport_begin + viewport_size

var _page_begin: int:
	get:
		return 0 if viewport_begin == 0 else viewport_begin + 1

var _page_size: int:
	get:
		var child_count = get_child_count()
		var offset := 0
		if viewport_begin > 0:
			offset -= 1
		if viewport_begin + viewport_size < child_count:
			offset -= 1
		return viewport_size + offset

var _page_end: int:
	get:
		return _page_begin + _page_size

func _update_viewport() -> void:
	if not is_node_ready(): await ready
	var child_count := get_child_count()
	for i in child_count:
		get_child(i).visible = i >= _page_begin and i < _page_end
	_up_arrow.visible = viewport_begin > 0
	_down_arrow.visible = viewport_end < child_count

var _up_arrow: TextureButton
var _down_arrow: TextureButton

func _init() -> void:
	_up_arrow = TextureButton.new()
	_up_arrow.name = &"UpArrow"
	_up_arrow.texture_normal = _arrow_texture
	_up_arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	_up_arrow.custom_minimum_size = Vector2(0, 8)
	add_child(_up_arrow, false, Node.INTERNAL_MODE_FRONT)
	_down_arrow = TextureButton.new()
	_down_arrow.name = &"DownArrow"
	_down_arrow.texture_normal = _arrow_texture
	_down_arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	_down_arrow.custom_minimum_size = Vector2(0, 8)
	_down_arrow.flip_v = true
	add_child(_down_arrow, false, Node.INTERNAL_MODE_BACK)

func _exit_tree() -> void:
	_up_arrow = null
