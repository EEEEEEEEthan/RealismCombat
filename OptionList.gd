@tool
extends MarginContainer
class_name OptionList

static var _默认指示器图标: Texture2D = ThemeDB.get_default_theme().get_icon("arrow_collapsed", "Tree")
static var _透明图标: ImageTexture = null
static var _空样式: StyleBoxEmpty = StyleBoxEmpty.new()

@export_range(3, 64) var 视区数量: int = 8:
	set(值):
		视区数量 = 值
		_尝试更新(_更新视区)

@export var _选项: PackedStringArray:
	set(值):
		if len(值) > 64:
			值 = 值.slice(0, 64)
		_选项 = 值
		_尝试更新(_更新视区)

@export var _禁用选项: int:
	set(值):
		_禁用选项 = 值
		_尝试更新(_更新视区)

@export_group("Theme Overrides")
@export_subgroup("icons")
@export var indexer_icon: Texture2D = null:
	set(值):
		indexer_icon = 值
		_尝试更新(_更新主题)

signal 当聚焦于选项(选项索引: int)
signal 当选择选项(选项索引: int)

var _选项容器: VBoxContainer

var _视区第一个编号: int

var _当前图标: Texture2D:
	get:
		if indexer_icon:
			return indexer_icon
		elif has_theme_icon("indexer_icon", "OptionList"):
			return get_theme_icon("indexer_icon", "OptionList")
		else:
			return _默认指示器图标

func _notification(通知类型: int) -> void:
	if 通知类型 == NOTIFICATION_THEME_CHANGED and is_node_ready():
		_更新主题()

func _ready() -> void:
	_选项容器 = VBoxContainer.new()
	_选项容器.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(_选项容器)
	_尝试更新(_更新主题)
	_尝试更新(_更新视区)

func _尝试更新(更新函数: Callable) -> void:
	if is_node_ready():
		更新函数.call()

func _更新主题() -> void:
	if _当前图标:
		var 图标尺寸 = max(_当前图标.get_width(), _当前图标.get_height())
		if _透明图标 == null or _透明图标.get_width() != 图标尺寸:
			var 图片 = Image.create(图标尺寸, 图标尺寸, false, Image.FORMAT_RGBA8)
			图片.fill(Color.TRANSPARENT)
			_透明图标 = ImageTexture.create_from_image(图片)
	_更新所有按钮图标()

func _更新视区() -> void:
	var 节点数量 = _选项容器.get_child_count()
	for 索引 in range(节点数量 - 视区数量):
		_选项容器.get_child(节点数量 - 索引 - 1).queue_free()
	for 索引 in range(视区数量 - 节点数量):
		_选项容器.add_child(_创建按钮())
	var 可见数量 = min(视区数量, len(_选项))
	for 索引 in range(可见数量):
		var 按钮 = _选项容器.get_child(索引) as Button
		if 索引 == 0 and _视区第一个编号 > 0:
			按钮.text = "...+" + str(_视区第一个编号 + 1)
		elif 视区数量 - 1 == 索引 and _视区第一个编号 + 视区数量 < len(_选项):
			按钮.text = "...+" + str(len(_选项) - (_视区第一个编号 + 视区数量) + 1)
		elif 索引 + _视区第一个编号 < len(_选项):
			按钮.text = _选项[索引 + _视区第一个编号]
		else:
			按钮.text = ""
		按钮.disabled = ((索引 & _禁用选项) != 0)
	for 索引 in range(可见数量, 视区数量):
		(_选项容器.get_child(索引) as Button).text = ""
	_更新所有按钮图标()

func _当按钮聚焦(按钮: Button) -> void:
	按钮.icon = _当前图标
	var 按钮下标 = 按钮.get_index()
	if 按钮下标 == 0 and _视区第一个编号 > 0:
		_视区第一个编号 -= 1
		按钮.get_parent().get_child(1).grab_focus()
		call_deferred("_更新视区")
	elif 按钮下标 == 视区数量 - 1 and _视区第一个编号 + 视区数量 < len(_选项):
		_视区第一个编号 += 1
		按钮.get_parent().get_child(按钮下标 - 1).grab_focus()
		call_deferred("_更新视区")
	else:
		var 选项索引 = _计算选项索引(按钮下标)
		if 选项索引 >= 0 and 选项索引 < len(_选项):
			当聚焦于选项.emit(选项索引)

func 按钮失焦时(按钮: Button) -> void:
	按钮.icon = _透明图标

func 鼠标进入按钮时(按钮: Button) -> void:
	按钮.grab_focus()

func 按钮按下时(按钮: Button) -> void:
	var 按钮下标 = 按钮.get_index()
	var 选项索引 = _计算选项索引(按钮下标)
	if not 按钮.disabled and 选项索引 >= 0 and 选项索引 < len(_选项) and not 按钮.text.begins_with("..."):
		当选择选项.emit(选项索引)

func _创建按钮() -> Button:
	var 按钮 = Button.new()
	按钮.focus_entered.connect(_当按钮聚焦.bind(按钮))
	按钮.focus_exited.connect(按钮失焦时.bind(按钮))
	按钮.mouse_entered.connect(鼠标进入按钮时.bind(按钮))
	按钮.pressed.connect(按钮按下时.bind(按钮))
	按钮.add_theme_stylebox_override("normal", _空样式)
	按钮.add_theme_stylebox_override("hover", _空样式)
	按钮.add_theme_stylebox_override("pressed", _空样式)
	按钮.add_theme_stylebox_override("disabled", _空样式)
	按钮.add_theme_stylebox_override("focus", _空样式)
	按钮.alignment = HORIZONTAL_ALIGNMENT_LEFT
	if _透明图标:
		按钮.icon = _透明图标
	return 按钮

func _计算选项索引(按钮下标: int) -> int:
	return 按钮下标 + _视区第一个编号

func _更新所有按钮图标() -> void:
	var 节点数量 = _选项容器.get_child_count()
	for i in range(节点数量):
		var 按钮 = _选项容器.get_child(i) as Button
		if 按钮:
			if 按钮.has_focus():
				按钮.icon = _当前图标
			else:
				按钮.icon = _透明图标
