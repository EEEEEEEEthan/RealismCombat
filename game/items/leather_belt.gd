extends Belt
class_name LeatherBelt

## 皮带：4 格武器槽。


func _init() -> void:
	super(4)
	_base_protection = Protection.new(2, 2, 2)


func get_name() -> StringName:
	return &"皮带"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "可挂载 4 把武器的腰带。"
