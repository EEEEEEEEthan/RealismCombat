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

@export_range(0, 32) var viewport_begin: int:
	set(value):
		if value == 1:
			value = 0
		viewport_begin = value
		_update_viewport()

@export_range(1, 8) var viewport_size: int:
	set(value):
		viewport_size = value
		_update_viewport()

var _true_viewport_begin: int:
	get:
		return 0 if viewport_begin == 0 else viewport_begin + 1

var _true_viewport_size: int:
	get:
		var child_count = get_child_count()
		var offset := 0
		if viewport_begin == 0:
			offset -= 1
		if viewport_begin + viewport_size > child_count:
			offset -= 1
		return viewport_size + offset

func _update_viewport() -> void:
	if not is_node_ready(): await ready
	var child_count := get_child_count()
	var viewport_end = _true_viewport_begin + _true_viewport_size
	for i in child_count:
		get_child(i).visible = i >= _true_viewport_begin and i < viewport_end
	_up_arrow.visible = viewport_begin > 0
	_down_arrow.visible = viewport_end < child_count
	print(viewport_end, "/", child_count)

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
