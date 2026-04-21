extends RefCounted
class_name EquipmentMenuFlow

## 主菜单「装备」：角色 → 身体部位 → 槽位列表 →（有物则）嵌套槽 / 卸下 / 返回；（空槽则）从背包匹配装上。

var _game: Game
var _nav := EquipmentNavigationStack.new()


func enter(game: Game) -> void:
	_game = game
	_nav.clear()
	await _menu_characters()


func _menu_characters() -> void:
	while true:
		var options: Array[MenuItemData] = []
		for player_character in _game.player_side_characters:
			options.append(MenuItemData.new("%s.." % player_character.character_name, false, "查看该角色装备"))
		var back_index := options.size()
		options.append(MenuItemData.new("返回", false, "回到主菜单"))
		var menu := Dialogues.create_menu_dialogue()
		menu.title = "装备"
		menu.options = options
		var choice = await menu.pressed
		menu.queue_free()
		if choice == back_index:
			return
		var character := _game.player_side_characters[choice]
		_nav.push(character)
		await _menu_body_parts(character)
		_nav.pop()


func _menu_body_parts(character: Character) -> void:
	while true:
		var options: Array[MenuItemData] = []
		for body_part in character.all_body_parts:
			options.append(MenuItemData.new("%s.." % body_part.part_name(), false, ""))
		var back_index := options.size()
		options.append(MenuItemData.new("返回", false, "角色列表"))
		var menu := Dialogues.create_menu_dialogue()
		menu.title = "装备>%s" % character.character_name
		menu.options = options
		var choice = await menu.pressed
		menu.queue_free()
		if choice == back_index:
			return
		var body_part := character.all_body_parts[choice]
		_nav.push(body_part)
		await _menu_slots_on_body_part(character, body_part)
		_nav.pop()


func _menu_slots_on_body_part(character: Character, body_part: BodyPart) -> void:
	var slots := body_part.get_item_slots()
	while true:
		var options: Array[MenuItemData] = []
		for slot in slots:
			var line := _slot_line(slot)
			var desc := slot.item.get_description() if slot.item else "空槽"
			options.append(MenuItemData.new("%s.." % line, false, desc))
		var back_index := options.size()
		options.append(MenuItemData.new("返回", false, "部位列表"))
		var menu := Dialogues.create_menu_dialogue()
		menu.title = "装备>%s>%s" % [character.character_name, body_part.part_name()]
		menu.options = options
		var choice = await menu.pressed
		menu.queue_free()
		if choice == back_index:
			return
		var picked_slot: ItemSlot = slots[choice]
		if picked_slot.item:
			await _menu_host_item(character, picked_slot.item, picked_slot)
		else:
			await _menu_pick_from_inventory(picked_slot, body_part)


func _menu_host_item(character: Character, host_item: Item, parent_slot: ItemSlot) -> void:
	var slots := host_item.get_item_slots()
	while true:
		var options: Array[MenuItemData] = []
		for slot in slots:
			var line := _slot_line(slot)
			var desc := slot.item.get_description() if slot.item else "空槽"
			options.append(MenuItemData.new("%s.." % line, false, desc))
		var unequip_index := options.size()
		options.append(MenuItemData.new("卸下", false, "将该物品放回背包"))
		var back_index := options.size()
		options.append(MenuItemData.new("返回", false, "上一级"))
		var menu := Dialogues.create_menu_dialogue()
		menu.title = "装备>%s>%s" % [character.character_name, str(host_item)]
		menu.options = options
		var choice = await menu.pressed
		menu.queue_free()
		if choice == back_index:
			return
		if choice == unequip_index:
			_unequip_slot_to_inventory(parent_slot)
			return
		var child_slot: ItemSlot = slots[choice]
		if child_slot.item:
			await _menu_host_item(character, child_slot.item, child_slot)
		else:
			await _menu_pick_from_inventory(child_slot)


func _menu_pick_from_inventory(slot: ItemSlot, lateral_body_part: BodyPart = null) -> void:
	while true:
		var matches := slot.matching_inventory_items(_game.inventory)
		var options: Array[MenuItemData] = []
		for candidate in matches:
			var side_mismatch := (
				lateral_body_part != null
				and not _item_side_matches_body_part(lateral_body_part, candidate)
			)
			options.append(MenuItemData.new("%s.." % candidate, side_mismatch, candidate.get_description()))
		var back_index := options.size()
		options.append(MenuItemData.new("返回", false, "取消"))
		var menu := Dialogues.create_menu_dialogue()
		menu.title = "选择物品装上"
		menu.options = options
		var choice = await menu.pressed
		menu.queue_free()
		if choice == back_index:
			return
		var picked_item: Item = matches[choice]
		slot.item = picked_item
		_game.inventory.remove_item(picked_item)
		return


func _item_side_matches_body_part(body_part: BodyPart, item: Item) -> bool:
	if body_part is Foot and item is Footwear:
		return (body_part as Foot).side == (item as Footwear).side
	if body_part is Hand and item is Glove:
		return (body_part as Hand).side == (item as Glove).side
	return true


func _slot_line(slot: ItemSlot) -> String:
	if slot.item:
		return str(slot.item)
	return "空"


func _unequip_slot_to_inventory(slot: ItemSlot) -> void:
	var unequipped: Item = slot.item
	if unequipped == null:
		return
	slot.item = null
	_game.inventory.add_item(unequipped)
