class_name ItemSlot

var _allowed_item_scripts: Array[Script]

var item: Item:
	set(v):
		if v != null and not _allowed_item_scripts.is_empty():
			var script_walk: Script = v.get_script() as Script
			var matched := false
			while script_walk:
				if _allowed_item_scripts.has(script_walk):
					matched = true
					break
				script_walk = script_walk.get_base_script()
			if not matched:
				push_error("物品类型与槽位不匹配")
				return
		item = v


func _init(allowed_item_scripts: Array[Script] = []) -> void:
	_allowed_item_scripts = allowed_item_scripts.duplicate()
