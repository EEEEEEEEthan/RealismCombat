extends Node
## 任意 Control 获得 GUI 焦点时打印其节点路径（调试用）


func _ready() -> void:
	var root_window: Window = get_tree().root
	root_window.gui_focus_changed.connect(_on_gui_focus_changed)


func _on_gui_focus_changed(control: Control) -> void:
	if control == null:
		print("[FocusLogger] (无焦点)")
		return
	print("[FocusLogger] ", control.get_path())
