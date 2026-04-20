extends OuterLayerArmor
class_name PlateArmorSuit

## 板甲（外层）。


func _init() -> void:
	super._init()
	_base_protection = Protection.new(22, 18, 16)


func get_name() -> StringName:
	return &"板甲"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "金属板片铆接的外层护甲，其上可穿罩袍或系腰带。"
