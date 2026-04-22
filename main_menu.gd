extends Node
class_name MainMenu


func _ready() -> void:
	var program: Program = get_parent()
	while true:
		var menu = Dialogues.create_menu_dialogue()
		menu.title = "Realism Combat"
		menu.options = [
			MenuItemData.new("新游戏..", false, "开启一段新的故事"),
			MenuItemData.new("读取游戏..", false, "读取已保存的进度"),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new("退出", false, "离开"),
		] as Array[MenuItemData]
		var index = await menu.pressed
		menu.visible = false
		match index:
			0:
				await _run_new_game_slots(program)
			1:
				await _run_load_slots(program)
			6:
				return
		menu.visible = true


func _run_new_game_slots(program: Program) -> void:
	while true:
		SaveSlots.debug_print_slot_state("新游戏选槽")
		var slot_menu := Dialogues.create_menu_dialogue()
		slot_menu.title = "新游戏 — 选择槽位"
		slot_menu.options = _make_new_game_slot_options()
		var choice: int = await slot_menu.pressed
		slot_menu.queue_free()
		await slot_menu.tree_exited
		if choice == SaveSlots.SLOT_COUNT:
			return
		if SaveSlots.file_exists(choice):
			if not await _confirm_overwrite(choice):
				continue
		var game := program.begin_new_game(choice)
		await game.tree_exiting
		return


func _confirm_overwrite(slot_index: int) -> bool:
	var overwrite_dialogue := Dialogues.create_generic_dialogue()
	overwrite_dialogue.text = "槽位 %d 已有存档，是否覆盖并开始新游戏？" % (slot_index + 1)
	overwrite_dialogue.options = [
		MenuItemData.new("覆盖并开始", false, "覆盖该槽并开始新游戏"),
		MenuItemData.new("返回选槽", false, "重新选择槽位"),
	]
	var answer: int = await overwrite_dialogue.pressed
	overwrite_dialogue.queue_free()
	return answer == 0


func _run_load_slots(program: Program) -> void:
	while true:
		SaveSlots.debug_print_slot_state("读取选槽")
		var slot_menu := Dialogues.create_menu_dialogue()
		slot_menu.title = "读取游戏 — 选择槽位"
		slot_menu.options = _make_load_slot_options()
		var choice: int = await slot_menu.pressed
		slot_menu.queue_free()
		await slot_menu.tree_exited
		if choice == SaveSlots.SLOT_COUNT:
			return
		var game := program.begin_loaded_game(choice)
		await game.tree_exiting
		return


func _make_new_game_slot_options() -> Array[MenuItemData]:
	var items: Array[MenuItemData] = []
	for slot_index in range(SaveSlots.SLOT_COUNT):
		var occupied := SaveSlots.file_exists(slot_index)
		var label := "存档 %d" % (slot_index + 1)
		if occupied:
			label += "（已有存档）"
		items.append(MenuItemData.new(
			label,
			false,
			"覆盖并开始新游戏" if occupied else "在此槽开始新游戏",
		))
	items.append(MenuItemData.new("返回", false, "返回主菜单"))
	return items


func _make_load_slot_options() -> Array[MenuItemData]:
	var items: Array[MenuItemData] = []
	for slot_index in range(SaveSlots.SLOT_COUNT):
		var readable := SaveSlots.file_exists(slot_index)
		items.append(MenuItemData.new(
			"存档 %d%s" % [slot_index + 1, " · 可读取" if readable else " · 空"],
			not readable,
			"读取此存档" if readable else "该槽没有存档",
		))
	items.append(MenuItemData.new("返回", false, "返回主菜单"))
	return items
