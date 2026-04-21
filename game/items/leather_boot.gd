extends Footwear
class_name LeatherBoot


func _init() -> void:
	super._init()
	_base_protection = Protection.new(3, 2, 3)


func get_name() -> StringName:
	return &"皮靴"


func get_title() -> StringName:
	return &""


func get_description() -> String:
	return "简易皮靴，分左右脚各穿一只。"
