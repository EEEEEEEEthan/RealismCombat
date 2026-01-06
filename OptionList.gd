@tool
extends MarginContainer
class_name OptionList

static var _默认指示器图标: Texture2D = ThemeDB.get_default_theme().get_icon("arrow_collapsed", "Tree")

@export_range(3, 16) var 视区数量: int = 8:
	set(v):
		视区数量 = v
		_延迟更新视区()

@export var 选项: PackedStringArray:
	set(v):
		选项 = v
		_延迟更新视区()

@export_range(1, 7) var 空余数量: int = 1:
	set(v):
		空余数量 = v
		_延迟更新视区()

@export var 指示器偏移: Vector2i:
	set(v):
		指示器偏移 = v
		_延迟更新视区()

@export_group("Theme Overrides")
@export_subgroup("icons")
@export var indexer_icon: Texture2D = null:
	set(value):
		indexer_icon = value
		_延迟更新主题()

var _选项容器: VBoxContainer
var _指示器: TextureRect
var _视区第一个编号: int
var _指示器序号: int

func _notification(what: int) -> void:
	if what == NOTIFICATION_THEME_CHANGED and is_node_ready():
		_更新主题()

func _ready() -> void:
	_选项容器 = VBoxContainer.new()
	_选项容器.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_选项容器)
	var control = Control.new()
	_指示器 = TextureRect.new()
	control.add_child(_指示器)
	add_child(control)
	call_deferred("_更新主题")
	call_deferred("_更新视区")

func _unhandled_key_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_up"):
		_处理向上移动()
	elif event.is_action_pressed("ui_down"):
		_处理向下移动()

func _处理向上移动() -> void:
	var margin = min(int((视区数量 - 1) / 2.0), 空余数量)
	_指示器序号 -= 1
	if _指示器序号 <= margin:
		if _视区第一个编号 > 0:
			_视区第一个编号 -= 1
			_指示器序号 += 1
		elif _指示器序号 < 0:
			_指示器序号 = 0
	_更新视区()

func _处理向下移动() -> void:
	var margin = min(int((视区数量 - 1) / 2.0), 空余数量)
	_指示器序号 += 1
	if _指示器序号 >= 视区数量 - margin - 1:
		if _视区第一个编号 + 视区数量 < len(选项):
			_视区第一个编号 += 1
			_指示器序号 -= 1
		elif _指示器序号 >= 视区数量:
			_指示器序号 = 视区数量 - 1
	_更新视区()

func _延迟更新视区() -> void:
	if is_node_ready():
		_更新视区()

func _延迟更新主题() -> void:
	if is_node_ready():
		_更新主题()

func _更新主题() -> void:
	if indexer_icon:
		_指示器.texture = indexer_icon
	elif has_theme_icon("indexer_icon", "OptionList"):
		_指示器.texture = get_theme_icon("indexer_icon", "OptionList")
	else:
		_指示器.texture = _默认指示器图标
	_更新指示器坐标()

func _更新视区() -> void:
	var 节点数量 = _选项容器.get_child_count()
	for i in range(节点数量 - 视区数量):
		_选项容器.get_child(节点数量 - i - 1).queue_free()
	for i in range(视区数量 - 节点数量):
		_选项容器.add_child(Label.new())
	var 可见数量 = min(视区数量, len(选项))
	for i in range(可见数量):
		if i == 0 and _视区第一个编号 > 0:
			_选项容器.get_child(i).text = "..."
		elif 视区数量 - 1 == i and _视区第一个编号 + 视区数量 < len(选项):
			_选项容器.get_child(i).text = "..."
		else:
			_选项容器.get_child(i).text = 选项[i + _视区第一个编号]
	for i in range(可见数量, 视区数量 - 可见数量):
		(_选项容器.get_child(i) as Label).text = ""
	_更新指示器坐标()

func _更新指示器坐标() -> void:
	var 节点数量 = _选项容器.get_child_count()
	if 节点数量 > 0:
		_指示器.visible = true
		var 节点 = _选项容器.get_child(clamp(_指示器序号, 0, 节点数量 - 1)) as Control
		var 坐标 = 节点.global_position
		坐标 += Vector2(-_指示器.size.x, (节点.size.y - _指示器.size.y) / 2)
		坐标 += 指示器偏移
		_指示器.global_position = 坐标
	else:
		_指示器.visible = false
