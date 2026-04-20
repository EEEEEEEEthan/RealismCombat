class_name ItemSlot

var _allowed_item_scripts: Array[Script]

var item: Item:
	set(v):
		if v != null and not allows_item(v):
			push_error("物品类型与槽位不匹配")
			return
		item = v


func _init(allowed_item_scripts: Array[Script] = []) -> void:
	_allowed_item_scripts = allowed_item_scripts.duplicate()


func allows_item(candidate: Item) -> bool:
	if _allowed_item_scripts.is_empty():
		return true
	var script_walk: Script = candidate.get_script() as Script
	while script_walk:
		if _allowed_item_scripts.has(script_walk):
			return true
		script_walk = script_walk.get_base_script()
	return false


func matching_inventory_items(inventory: Inventory) -> Array[Item]:
	var out: Array[Item] = []
	for inv_item in inventory.items:
		if allows_item(inv_item):
			out.append(inv_item)
	return out
