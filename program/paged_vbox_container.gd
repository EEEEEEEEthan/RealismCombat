@tool
extends VBoxContainer
class_name PagedVBoxContainer

const ARROW_REGION_BASE_Y: float = 2.0
const INDEXER_ARROW_FOCUS_REGION = Rect2(11, 1, 9, 8)
const INDEXER_ARROW_PRESSED_REGION = Rect2(10, 1, 9, 8)

static var _theme_atlas_cache: Texture2D

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
	var viewport_end := viewport_begin + _viewport_height
	for i in child_count:
		get_child(i).visible = i >= viewport_begin and i < viewport_end
	_up_arrow.visible = _up_arrow_visible
	_down_arrow.visible = _down_arrow_visible
	_update_indexer()

var _up_arrow: TextureButton
var _down_arrow: TextureButton

static func _load_theme_atlas() -> Texture2D:
	if not _theme_atlas_cache:
		_theme_atlas_cache = ResourceLoader.load("uid://dkajou5ypu7d0")
	return _theme_atlas_cache

static func _get_tracked_signal_names(node: Node) -> Array[StringName]:
	if node is Button:
		return [&"focus_entered", &"focus_exited", &"button_down", &"button_up"]
	return [&"focus_entered", &"focus_exited"]

static func _create_arrow_texture() -> AtlasTexture:
	var atlas := _load_theme_atlas()
	var tex := AtlasTexture.new()
	tex.atlas = atlas
	tex.region = Rect2(21, ARROW_REGION_BASE_Y, 8, 6)
	return tex

var _indexer_control: Control
var _indexer_arrow_texture: AtlasTexture

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
	_up_arrow.pressed.connect(func(): viewport_begin -= 1)
	add_child(_up_arrow, false, Node.INTERNAL_MODE_FRONT)
	_down_arrow = TextureButton.new()
	_down_arrow.name = &"DownArrow"
	_down_arrow.texture_normal = _create_arrow_texture()
	_down_arrow.stretch_mode = TextureButton.STRETCH_KEEP_CENTERED
	_down_arrow.custom_minimum_size = Vector2(0, 8)
	_down_arrow.flip_v = true
	_down_arrow.button_down.connect(shift_arrow.bind(_down_arrow, ARROW_REGION_BASE_Y + 1))
	_down_arrow.button_up.connect(shift_arrow.bind(_down_arrow, ARROW_REGION_BASE_Y))
	_down_arrow.pressed.connect(func(): viewport_begin += 1)
	add_child(_down_arrow, false, Node.INTERNAL_MODE_BACK)

	_indexer_control = Control.new()
	_indexer_control.top_level = true
	_indexer_control.mouse_filter = MOUSE_FILTER_IGNORE
	_indexer_control.name = &"IndexerControl"
	_indexer_arrow_texture = AtlasTexture.new()
	_indexer_arrow_texture.atlas = _load_theme_atlas()
	_indexer_arrow_texture.region = INDEXER_ARROW_FOCUS_REGION
	var _indexer_arrow := TextureRect.new()
	_indexer_arrow.name = &"IndexerArrow"
	_indexer_arrow.texture = _indexer_arrow_texture
	_indexer_arrow.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	_indexer_arrow.position = Vector2(-9, 0)
	_indexer_arrow.size = Vector2(9, 8)
	_indexer_control.add_child(_indexer_arrow)
	add_child(_indexer_control, false, Node.INTERNAL_MODE_BACK)
	_indexer_control.visible = false

func _ready() -> void:
	child_entered_tree.connect(_on_child_entered_tree)
	child_exiting_tree.connect(_on_child_exiting_tree)
	resized.connect(_update_indexer)
	for child in get_children():
		if _is_internal_node(child):
			continue
		if child is Control:
			_connect_child_signals(child)
	_update_viewport()
	_update_indexer()

func _exit_tree() -> void:
	_up_arrow = null
	_down_arrow = null
	_indexer_control = null
	_indexer_arrow_texture = null

func _is_internal_node(node: Node) -> bool:
	return node == _up_arrow or node == _down_arrow or node == _indexer_control

func _content_child_count() -> int:
	return get_child_count() - 3

func _connect_child_signals(child: Node) -> void:
	for signal_name in _get_tracked_signal_names(child):
		child.connect(signal_name, _update_indexer)

func _disconnect_child_signals(child: Node) -> void:
	for signal_name in _get_tracked_signal_names(child):
		child.disconnect(signal_name, _update_indexer)

func _on_child_entered_tree(child: Node) -> void:
	if _is_internal_node(child):
		return
	if child is Control:
		_connect_child_signals(child)
		_update_indexer()

func _on_child_exiting_tree(child: Node) -> void:
	if _is_internal_node(child):
		return
	if child is Control:
		_disconnect_child_signals(child)
		_update_indexer()

func _update_indexer() -> void:
	if not is_node_ready():
		return
	var focused := get_viewport().gui_get_focus_owner()
	if not focused or not is_ancestor_of(focused) or _is_internal_node(focused) or not focused.visible:
		_indexer_control.visible = false
		return
	_indexer_control.visible = true
	_update_indexer_deferred.call_deferred(focused)

func _update_indexer_deferred(focused: Control) -> void:
	if not is_instance_valid(focused) or not is_instance_valid(_indexer_control):
		return
	_indexer_control.global_position = focused.global_position
	_indexer_control.size = focused.size
	if focused is Button and focused.button_pressed:
		_indexer_arrow_texture.region = INDEXER_ARROW_PRESSED_REGION
	else:
		_indexer_arrow_texture.region = INDEXER_ARROW_FOCUS_REGION