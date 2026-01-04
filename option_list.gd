@tool
extends VBoxContainer
class_name OptionList

@export_range(3, 16) var viewport_count: int:
	set(v):
		viewport_count = v
		if is_node_ready():
			_update()

@export var options: PackedStringArray:
	set(v):
		options = v
		if is_node_ready():
			_update()

@export_range(0, 65535) var begin: int

func _ready() -> void:
	_update()

func _update() -> void:
	var child_count = get_child_count()
	for i in range(child_count - viewport_count):
		get_child(child_count - i - 1).queue_free()
	for i in range(viewport_count - child_count):
		add_child(Label.new())
	var visible_count = min(viewport_count, len(options))
	for i in range(begin, visible_count):
		if begin == i and begin > 0:
			get_child(i).text = "More..."
		elif begin + viewport_count == i and begin + viewport_count < len(options) - 1:
			get_child(i).text = "More..."
		get_child(i).text = options[i]
	for i in range(visible_count, viewport_count - visible_count):
		(get_child(i) as Label).text = ""
