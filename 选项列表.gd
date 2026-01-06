@tool
extends MarginContainer
class_name 选项列表

@export_range(3, 16) var 视区数量: int = 8:
	set(v):
		视区数量 = v
		if is_node_ready():
			_更新视觉()

@export var 选项: PackedStringArray:
	set(v):
		选项 = v
		if is_node_ready():
			_更新视觉()

@export_range(1, 7) var 空余数量: int = 1:
	set(v):
		空余数量 = v
		if is_node_ready():
			_更新视觉()

@export var 指示器偏移: Vector2i:
	set(v):
		指示器偏移 = v;
		if is_node_ready():
			_更新视觉()

var _选项容器: VBoxContainer
var _指示器: TextureRect
var _视区第一个编号: int
var _指示器序号: int

func _ready() -> void:
	_选项容器 = VBoxContainer.new()
	_选项容器.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_选项容器)
	var control = Control.new()
	_指示器 = TextureRect.new()
	control.add_child(_指示器)
	add_child(control)
	var 图集 = AtlasTexture.new()
	图集.atlas = ResourceLoader.load("res://图集.png")
	图集.region = Rect2i(0, 0, 8, 8)
	_指示器.texture = 图集
	call_deferred("_更新视觉")

func _unhandled_key_input(event: InputEvent) -> void:
	var margin = min((视区数量 - 1) / 2, 空余数量)
	if event.is_action_pressed("ui_up"):
		_指示器序号 -= 1
		if _指示器序号 <= margin:
			if _视区第一个编号 > 0:
				_视区第一个编号 -= 1
				_指示器序号 += 1
			elif _指示器序号 < 0:
				_指示器序号 = 0
		_更新视觉()
	elif event.is_action_pressed("ui_down"):
		_指示器序号 += 1
		if _指示器序号 >= 视区数量 - margin - 1:
			if _视区第一个编号 + 视区数量 < len(选项):
				_视区第一个编号 += 1
				_指示器序号 -= 1
			elif _指示器序号 >= 视区数量:
				_指示器序号 = 视区数量 - 1
		_更新视觉()

func _更新视觉() -> void:
	var 节点数量 = _选项容器.get_child_count()
	for i in range(节点数量 - 视区数量):
		_选项容器.get_child(节点数量 - i - 1).queue_free()
	print(视区数量)
	for i in range(视区数量 - 节点数量):
		_选项容器.add_child(Label.new())
	节点数量 = _选项容器.get_child_count()
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
	if 节点数量 > 0:
		_指示器.visible = true
		var 节点 = _选项容器.get_child(clamp(_指示器序号, 0, 节点数量)) as Control
		var 坐标 = 节点.global_position;
		坐标 += Vector2(-_指示器.size.x, (节点.size.y - _指示器.size.y) / 2)
		_指示器.global_position = 坐标
	else:
		_指示器.visible = false
