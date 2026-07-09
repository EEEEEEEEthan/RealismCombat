@tool
extends VBoxContainer
class_name OptionContainer

# 箭头纹理在 Atlas 中的裁剪区域
const _ARROW_TEX_REGION := Rect2(21, 2, 8, 5)
# 箭头最小高度（与 Atlas 中箭头像素高度一致）
const _ARROW_MIN_HEIGHT := 8.0

var _arrow_texture: AtlasTexture:
	get:
		if not _arrow_texture:
			var image = ResourceLoader.load("uid://dkajou5ypu7d0") as Texture2D
			assert(image, "箭头图集资源未找到")
			_arrow_texture = AtlasTexture.new()
			_arrow_texture.atlas = image
			_arrow_texture.region = _ARROW_TEX_REGION
		return _arrow_texture

@export_range(0, 32) var viewport_begin: int:
	get:
		return _viewport_begin
	set(value):
		_viewport_begin = value
		_update_viewport()

@export_range(1, 8) var viewport_size: int:
	get:
		return _viewport_size
	set(value):
		_viewport_size = value
		_update_viewport()

var _viewport_begin: int = 0
var _viewport_size: int = 1

var _up_arrow: TextureButton
var _down_arrow: TextureButton


func _init() -> void:
	_up_arrow = _build_arrow(&"UpArrow", _arrow_texture, false)
	add_child(_up_arrow, false, Node.INTERNAL_MODE_FRONT)

	_down_arrow = _build_arrow(&"DownArrow", _arrow_texture, true)
	add_child(_down_arrow, false, Node.INTERNAL_MODE_BACK)


func _exit_tree() -> void:
	_up_arrow = null
	_down_arrow = null


static func _build_arrow(name: StringName, texture: Texture2D, flipped: bool) -> TextureButton:
	var arrow := TextureButton.new()
	arrow.name = name
	arrow.texture_normal = texture
	arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	arrow.custom_minimum_size = Vector2(0, _ARROW_MIN_HEIGHT)
	arrow.flip_v = flipped
	return arrow


func _update_viewport() -> void:
	if not is_node_ready():
		await ready

	# 内容子节点数 = 外部节点数（不包含内部箭头）
	var content_count := get_child_count(false)
	if content_count <= 0:
		return

	# 将视口起始位置限制在有效范围内
	var first_visible := clampi(_viewport_begin, 0, content_count - 1)
	# 回写 clamped 值，保持 viewport_begin 与有效状态同步
	_viewport_begin = first_visible

	# 上箭头：非起始位置时显示，占一个可见槽位
	var show_up := first_visible > 0
	var content_slots := _viewport_size - (1 if show_up else 0)

	# 下箭头：内容未到底时显示，占一个可见槽位
	var show_down := first_visible + content_slots < content_count
	if show_down:
		content_slots -= 1

	_up_arrow.visible = show_up
	_down_arrow.visible = show_down

	# 计算可见内容索引范围 [first_visible, last_visible)
	var last_visible := first_visible + content_slots

	# 更新内容子节点可见性
	# content_index 即是外部节点索引（0-based），与 get_child(false) 一致
	for content_index in content_count:
		var child := get_child(content_index, false)
		child.visible = content_index >= first_visible and content_index < last_visible
