@abstract
extends Item
class_name Glove

## 手套：单件装备于一只手，无分层嵌套。
var side: Defs.Side


func _init(p_side: Defs.Side) -> void:
	super._init()
	side = p_side
