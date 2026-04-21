@abstract
extends Item
class_name Footwear

## 鞋靴：单件装备于一只脚，无分层嵌套。
var side: Defs.Side


func _init(p_side: Defs.Side) -> void:
	super._init()
	side = p_side
