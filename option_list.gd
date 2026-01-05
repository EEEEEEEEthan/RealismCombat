@tool
extends Control
class_name OptionList

@export_range(3, 16) var 视区数量: int:
	set(v):
		视区数量 = v
		if is_node_ready():
			_更新视觉()

@export var 选项: PackedStringArray:
	set(v):
		选项 = v
		if is_node_ready():
			_更新视觉()

@export_range(1, 7) var 空余数量: int:
	set(v):
		空余数量 = v
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
	_指示器 = TextureRect.new()
	add_child(_指示器)
	var 图集 = AtlasTexture.new()
	图集.atlas = ResourceLoader.load("res://图集.png")
	图集.region = Rect2i(0, 0, 8, 8)
	_指示器.texture = 图集
	_更新视觉()

func _unhandled_key_input(event: InputEvent) -> void:
	if event.is_action_pressed("ui_up"):
		_指示器序号 -= 1
		if _指示器序号 < 空余数量:
			if _视区第一个编号 > 0:
				_视区第一个编号 -= 1
				_指示器序号 += 1
			elif _指示器序号 < 0:
				_指示器序号 = 0
		_更新视觉()
	elif event.is_action_pressed("ui_down"):
		_指示器序号 += 1
		if _指示器序号 >= 视区数量 - 空余数量:
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
	for i in range(视区数量 - 节点数量):
		_选项容器.add_child(Label.new())
	节点数量 = _选项容器.get_child_count()
	var 可见数量 = min(视区数量, len(选项))
	for i in range(可见数量):
		if i == 0 and _视区第一个编号 > 0:
			_选项容器.get_child(i).text = "More..."
		elif 视区数量 - 1 == i and _视区第一个编号 + 视区数量 < len(选项):
			_选项容器.get_child(i).text = "More..."
		else:
			_选项容器.get_child(i).text = 选项[i + _视区第一个编号]
	for i in range(可见数量, 视区数量 - 可见数量):
		(_选项容器.get_child(i) as Label).text = ""
	_指示器.global_position = (_选项容器.get_child(clamp(_指示器序号, 0, 节点数量)) as Control).global_position
