extends Node
class_name Program


func _ready() -> void:
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
	while true:
		var index = await menu.pressed
		menu.visible = false
		match index:
			0:
				await _run_new_game_slots()
			1:
				await _run_load_slots()
			6:
				get_tree().quit()
		menu.visible = true


func begin_new_game(slot_index: int) -> Game:
	var game: Game = %Game.create_instance()
	game.path = SaveSlots.path_for_slot(slot_index)
	game.new_game()
	return game


func begin_loaded_game(slot_index: int) -> Game:
	var game: Game = %Game.create_instance()
	game.load_game(SaveSlots.path_for_slot(slot_index))
	return game


func _run_new_game_slots() -> void:
	SaveSlots.debug_print_slot_state("新游戏选槽")
	var slot_menu := Dialogues.create_menu_dialogue()
	slot_menu.title = "新游戏 — 选择槽位"
	slot_menu.options = SaveSlotPresentation.build_table_rows(false)
	while true:
		var choice: int = await slot_menu.pressed
		if choice == SaveSlots.SLOT_COUNT:
			slot_menu.queue_free()
			await slot_menu.tree_exited
			return
		if SaveSlots.file_exists(choice):
			slot_menu.visible = false
			if not await _confirm_overwrite(choice):
				slot_menu.visible = true
				continue
		slot_menu.queue_free()
		await slot_menu.tree_exited
		var game := begin_new_game(choice)
		await game.tree_exiting
		return


func _confirm_overwrite(slot_index: int) -> bool:
	var overwrite_dialogue := Dialogues.create_generic_dialogue()
	overwrite_dialogue.text = "#%d 已有存档，是否覆盖并开始新游戏？" % (slot_index + 1)
	overwrite_dialogue.options = [
		MenuItemData.new("覆盖并开始", false, "覆盖该槽并开始新游戏"),
		MenuItemData.new("返回选槽", false, "重新选择槽位"),
	]
	var answer: int = await overwrite_dialogue.pressed
	overwrite_dialogue.queue_free()
	return answer == 0


func _run_load_slots() -> void:
	while true:
		SaveSlots.debug_print_slot_state("读取选槽")
		var slot_menu := Dialogues.create_menu_dialogue()
		slot_menu.title = "读取游戏 — 选择槽位"
		slot_menu.options = SaveSlotPresentation.build_table_rows(true)
		var choice: int = await slot_menu.pressed
		slot_menu.queue_free()
		await slot_menu.tree_exited
		if choice == SaveSlots.SLOT_COUNT:
			return
		var game := begin_loaded_game(choice)
		await game.tree_exiting
		return
