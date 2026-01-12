@tool
extends MarginContainer
class_name OptionList

static var _默认指示器图标: Texture2D = ThemeDB.get_default_theme().get_icon("arrow_collapsed", "Tree")

@export_range(3, 16) var 视区数量: int = 8:
	set(值):
		视区数量 = 值
		_延迟更新视区()

@export var 选项: PackedStringArray:
	set(值):
		选项 = 值
		_延迟更新视区()

@export_range(1, 7) var 空余数量: int = 1:
	set(值):
		空余数量 = 值
		_延迟更新视区()

@export var 指示器偏移: Vector2i:
	set(值):
		指示器偏移 = 值
		_延迟更新视区()

@export_group("Theme Overrides")
@export_subgroup("icons")
@export var 指示器图标: Texture2D = null:
	set(值):
		指示器图标 = 值
		_延迟更新主题()

var _选项容器: VBoxContainer
var _指示器: TextureRect
var _视区第一个编号: int
var _指示器序号: int:
	get:
		var 子节点数量 = _选项容器.get_child_count()
		for i in range(子节点数量):
			if _选项容器.get_child(i).has_focus():
				return i;
		return -1;

func _notification(通知类型: int) -> void:
	if 通知类型 == NOTIFICATION_THEME_CHANGED and is_node_ready():
		_更新主题()

func _ready() -> void:
	_选项容器 = VBoxContainer.new()
	_选项容器.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_选项容器)
	var 控件 = Control.new()
	控件.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_指示器 = TextureRect.new()
	控件.add_child(_指示器)
	add_child(控件)
	call_deferred("_更新主题")
	call_deferred("_更新视区")

func _延迟更新视区() -> void:
	if is_node_ready():
		_更新视区()

func _延迟更新主题() -> void:
	if is_node_ready():
		_更新主题()

func _更新主题() -> void:
	if 指示器图标:
		_指示器.texture = 指示器图标
	elif has_theme_icon("indexer_icon", "OptionList"):
		_指示器.texture = get_theme_icon("indexer_icon", "OptionList")
	else:
		_指示器.texture = _默认指示器图标
	_更新指示器坐标()

func _更新视区() -> void:
	var 节点数量 = _选项容器.get_child_count()
	for 索引 in range(节点数量 - 视区数量):
		_选项容器.get_child(节点数量 - 索引 - 1).queue_free()
	for 索引 in range(视区数量 - 节点数量):
		var 按钮 = Button.new()
		按钮.focus_entered.connect(Callable(self, "_更新指示器坐标"))
		按钮.focus_exited.connect(Callable(self, "_更新指示器坐标"))
		_选项容器.add_child(按钮)
	var 可见数量 = min(视区数量, len(选项))
	for 索引 in range(可见数量):
		if 索引 == 0 and _视区第一个编号 > 0:
			_选项容器.get_child(索引).text = "..."
		elif 视区数量 - 1 == 索引 and _视区第一个编号 + 视区数量 < len(选项):
			_选项容器.get_child(索引).text = "..."
		elif 索引 + _视区第一个编号 < len(选项):
			_选项容器.get_child(索引).text = 选项[索引 + _视区第一个编号]
		else:
			_选项容器.get_child(索引).text = ""
	for 索引 in range(可见数量, 视区数量 - 可见数量):
		(_选项容器.get_child(索引) as Button).text = ""
	_更新指示器坐标()

func _更新指示器坐标() -> void:
	var 节点数量 = _选项容器.get_child_count()
	if 节点数量 > 0:
		_指示器.visible = true
		var 节点 = _选项容器.get_child(clamp(_指示器序号, 0, 节点数量 - 1)) as Control
		var 坐标 = 节点.global_position
		坐标 += Vector2(-_指示器.size.x, (节点.size.y - _指示器.size.y) / 2)
		坐标 += Vector2(指示器偏移)
		_指示器.global_position = 坐标
	else:
		_指示器.visible = false
