extends RefCounted
class_name Inventory

var items: Array[Item] = []


func add_item(item: Item) -> void:
	items.append(item)


func remove_item(item: Item) -> void:
	var index := items.find(item)
	if index >= 0:
		items.remove_at(index)


func to_menu_options() -> Array[MenuItemData]:
	var options: Array[MenuItemData] = []
	for inventory_item in items:
		options.append(MenuItemData.new(
			str(inventory_item),
			false,
			inventory_item.get_description(),
		))
	options.append(MenuItemData.new("返回", false, "关闭物品栏"))
	return options
