@tool
extends VBoxContainer
class_name PagedVBoxContainer

const ARROW_REGION_BASE_Y: float = 2.0

@export_range(0, 32, 1, "prefer_slider") var viewport_begin: int:
	get:
		if viewport_begin < 0: return 0
		var child_count = get_child_count()
		if child_count <= height: return 0
		var result = viewport_begin
		result = min(child_count - height + 1, result)
		return result
	set(value):
		viewport_begin = value
		_update_viewport()

@export_range(3, 16, 1, "prefer_slider") var height: int:
	set(value):
		height = value
		_update_viewport()

var _up_arrow_visible: bool:
	get:
		return viewport_begin > 0

var _down_arrow_visible: bool:
	get:
		var viewport_height = height
		if _up_arrow_visible: viewport_height -= 1
		return viewport_begin + viewport_height < get_child_count()

var _viewport_height: int:
	get:
		var result = height
		if _up_arrow_visible: result -= 1
		if _down_arrow_visible: result -= 1
		return result

func _update_viewport() -> void:
	if not is_node_ready(): await ready
	var child_count := get_child_count()
	var viewport_end = viewport_begin + _viewport_height
	for i in child_count:
		get_child(i).visible = i >= viewport_begin and i < viewport_end
	_up_arrow.visible = _up_arrow_visible
	_down_arrow.visible = _down_arrow_visible

var _up_arrow: TextureButton
var _down_arrow: TextureButton

static func _create_arrow_texture() -> AtlasTexture:
	var image = ResourceLoader.load("uid://dkajou5ypu7d0") as Texture2D
	assert(image)
	var tex := AtlasTexture.new()
	tex.atlas = image
	tex.region = Rect2(21, ARROW_REGION_BASE_Y, 8, 6)
	return tex

func _init() -> void:
	var shift_arrow := func(btn: TextureButton, y: float) -> void:
		var r: Rect2 = btn.texture_normal.region
		btn.texture_normal.region = Rect2(r.position.x, y, r.size.x, r.size.y)

	_up_arrow = TextureButton.new()
	_up_arrow.name = &"UpArrow"
	_up_arrow.texture_normal = _create_arrow_texture()
	_up_arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	_up_arrow.custom_minimum_size = Vector2(0, 8)
	_up_arrow.button_down.connect(shift_arrow.bind(_up_arrow, ARROW_REGION_BASE_Y - 1))
	_up_arrow.button_up.connect(shift_arrow.bind(_up_arrow, ARROW_REGION_BASE_Y))
	add_child(_up_arrow, false, Node.INTERNAL_MODE_FRONT)
	_down_arrow = TextureButton.new()
	_down_arrow.name = &"DownArrow"
	_down_arrow.texture_normal = _create_arrow_texture()
	_down_arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	_down_arrow.custom_minimum_size = Vector2(0, 8)
	_down_arrow.flip_v = true
	_down_arrow.button_down.connect(shift_arrow.bind(_down_arrow, ARROW_REGION_BASE_Y + 1))
	_down_arrow.button_up.connect(shift_arrow.bind(_down_arrow, ARROW_REGION_BASE_Y))
	add_child(_down_arrow, false, Node.INTERNAL_MODE_BACK)

func _exit_tree() -> void:
	_up_arrow = null
	_down_arrow = null
