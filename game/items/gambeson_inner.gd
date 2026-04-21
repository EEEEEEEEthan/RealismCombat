extends InnerLiningGarment
class_name GambesonInner

## 夹层棉质内衬（棉甲内层）。


func _init() -> void:
	super._init()
	_base_protection = Protection.new(5, 4, 10)


func get_name() -> StringName:
	return &"夹层内衬"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "贴身棉絮夹层，可再穿中层护甲、外套或腰带。"
