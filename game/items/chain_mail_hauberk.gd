extends MiddleLayerArmor
class_name ChainMailHauberk

## 锁子甲（中层）。


func _init() -> void:
	super._init()
	_base_protection = Protection.new(14, 16, 8)


func get_name() -> StringName:
	return &"锁甲"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "铁环编缀的中层护甲，其上可穿板甲、外套或腰带。"
