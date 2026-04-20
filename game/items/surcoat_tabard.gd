extends OuterwearCoat
class_name SurcoatTabard

## 罩袍式外套。


func _init() -> void:
	super._init()
	_base_protection = Protection.new(2, 2, 2)


func get_name() -> StringName:
	return &"罩袍"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "罩在铠甲外的布面外套，其上仅可系腰带。"
