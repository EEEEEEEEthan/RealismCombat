@tool
extends Range
class_name RetroProgressBar

static var _fill_texture: ImageTexture:
	get:
		if not _fill_texture:
			var iamge = Image.create(2, 6, false,Image.FORMAT_RGBA8)
			var white_points = [1, 2, 3, 4]
			for x in 2:
				for y in 6:
					if x == 0 and y in white_points:
						iamge.set_pixel(x, y, Color.WHITE)
					else:
						iamge.set_pixel(x, y, Color.TRANSPARENT)
			_fill_texture = ImageTexture.create_from_image(iamge)
		return _fill_texture

static var _back_texture: AtlasTexture:
	get:
		if not _back_texture:
			_back_texture = AtlasTexture.new()
			_back_texture.atlas = _fill_texture
			_back_texture.region = Rect2(0, 1, 2, 1)
		return _back_texture

static var _material: ShaderMaterial:
	get:
		if not _material:
			var shader = Shader.new()
			shader.code = """
shader_type canvas_item;
const float DIRECTION_REPICK_SECONDS = 0.3;
varying float local_vertex_x;
void vertex() {
	local_vertex_x = VERTEX.x;
}
float hash_column(float key_x) {
	float x = fract(key_x * 0.1031);
	x *= x + 33.33;
	return fract(x * x);
}
void fragment() {
	float column_x = floor(local_vertex_x);
	float time_slice = floor(TIME / DIRECTION_REPICK_SECONDS);
	float random_unit = hash_column(column_x + time_slice * 419.431);
	float above_first_third = step(1.0 / 3.0, random_unit);
	float above_second_third = step(2.0 / 3.0, random_unit);
	float direction = -1.0 + above_first_third + above_second_third;
	vec2 sample_uv = UV + vec2(0.0, direction * TEXTURE_PIXEL_SIZE.y);
	COLOR = texture(TEXTURE, sample_uv);
}
"""
			_material = ShaderMaterial.new()
			_material.shader = shader
		return _material

@export var jump: bool:
	set(v):
		jump = v
		_fill.material = _material if v else null

var _background: NinePatchRect
var _fill: NinePatchRect

func _init() -> void:
	# 须在任意 layout 通知或属性访问前建好子节点；懒加载 getter 会在祖先 blocked>0 时 add_child 失败
	_background = NinePatchRect.new()
	_background.region_rect = Rect2(0, 0, 2, 1)
	_background.patch_margin_top = 1
	_background.patch_margin_bottom = 1
	_background.axis_stretch_horizontal = NinePatchRect.AXIS_STRETCH_MODE_TILE
	_background.pivot_offset_ratio = Vector2(.5, .5)
	_background.texture = _back_texture
	_background.self_modulate = Color("797979")
	add_child(_background)
	_background.set_anchor_and_offset(SIDE_TOP, 0, 1)
	_background.set_anchor_and_offset(SIDE_RIGHT, 1, 0)
	_background.set_anchor_and_offset(SIDE_BOTTOM, 1, -1)
	_fill = NinePatchRect.new()
	_fill.region_rect = Rect2(0, 0, 2, 6)
	_fill.patch_margin_top = 3
	_fill.patch_margin_bottom = 3
	_fill.axis_stretch_horizontal = NinePatchRect.AXIS_STRETCH_MODE_TILE
	_fill.pivot_offset_ratio = Vector2(.5, .5)
	_fill.texture = _fill_texture
	_fill.layout_direction = Control.LAYOUT_DIRECTION_LTR
	_background.add_child(_fill)
	_fill.set_anchor_and_offset(SIDE_TOP, 0, -2)
	_fill.set_anchor_and_offset(SIDE_BOTTOM, 1, 2)

func _notification(what: int) -> void:
	if what == NOTIFICATION_LAYOUT_DIRECTION_CHANGED:
		_update_layout_direction()

func _get_minimum_size() -> Vector2:
	return Vector2(3, 4)

func _ready() -> void:
	_update_layout_direction()
	_update_progress()
	value_changed.connect(func (_v) -> void: _update_progress())
	changed.connect(_update_progress)

func _update_layout_direction() -> void:
	if is_layout_rtl():
		_background.scale = Vector2(-1, 1)
	else:
		_background.scale = Vector2(1, 1)

func _update_progress() -> void:
	_fill.set_anchor_and_offset(SIDE_RIGHT, value / max_value, 0)
