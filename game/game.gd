extends Node
class_name Game

var combat: Combat
## 当前玩家方参战角色（顺序即编队顺序）
var player_side_characters: Array[Character] = []
var inventory: Inventory
var path: String

## 由当前头字段在访问时新构造，非缓存同一实例
var snapshot: SaveSnapshot:
	get: return SaveSnapshot.new(
		GameVersion.CURRENT, "未命名", Time.get_unix_time_from_system(),
	)

func new_game() -> void:
	var ethan = Character.create_default(self, "Ethan")
	var rowan = Character.create_default(self, "Rowan")
	player_side_characters = [ethan, rowan]

func load_game(p_path) -> void:
	path = p_path
	var file_access: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file_access == null:
		return
	var header: SaveSnapshot = SaveSnapshot.read_header_from_file(file_access)
	if header == null:
		push_error("读档头失败: %s" % path)
		file_access.close()
		return
	var size = file_access.get_8()
	for i in size:
		player_side_characters.append(
			Character.create_deserialize(self, file_access),
		)
	file_access.close()

func _save_game() -> void:
	var dialogue = Dialogues.create_generic_dialogue()
	dialogue.text = "游戏已保存"
	var written: SaveSnapshot = snapshot
	var file_access: FileAccess = FileAccess.open(path, FileAccess.WRITE)
	if file_access == null:
		return
	SaveSnapshot.write_to_file(file_access, written)
	file_access.store_8(len(player_side_characters))
	for character in player_side_characters:
		character.serialize(file_access)
	await dialogue.pressed
	dialogue.queue_free()

func _ready() -> void:
	AudioManager.play_background_music(%Audios.menu_music)
	inventory = Inventory.new()
	var menu = Dialogues.create_menu_dialogue()
	menu.title = "Realism Combat"
	menu.options = [
		MenuItemData.new("测试项..", false, "测试项"),
		MenuItemData.new("装备..", false, "为角色装备或卸下物品"),
		MenuItemData.new("物品栏..", false, "查看持有的道具"),
		MenuItemData.new("保存", false, "写入当前进度到存档文件"),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new("返回菜单", false, "离开"),
	] as Array[MenuItemData]
	while true:
		menu.visible = true
		var main_menu_choice: int = await menu.pressed
		menu.visible = false
		match main_menu_choice:
			0: await _run_test_combat()
			1: await _run_equipment_menu()
			2: await _run_inventory_menu()
			3: await _save_game()
			6:
				menu.queue_free()
				queue_free()
				return

func _run_test_combat() -> void:
	combat = %Combat.create_instance()
	for player_character in player_side_characters:
		combat.add_character(player_character, Combat.PLAYER_SIDE)
	var dove: Character = Character.create_default(self, "Dove")
	combat.add_character(dove, Combat.ENEMY_SIDE)
	await combat.run()
	AudioManager.play_background_music(%Audios.menu_music)
	combat = null

func _run_equipment_menu() -> void:
	var flow := EquipmentMenuFlow.new()
	await flow.enter(self)

func _run_inventory_menu() -> void:
	var inventory_menu = Dialogues.create_menu_dialogue()
	inventory_menu.title = "物品栏"
	inventory_menu.options = inventory.to_menu_options()
	while true:
		var inventory_choice = await inventory_menu.pressed
		if inventory_choice == inventory.items.size():
			break
	inventory_menu.queue_free()

func _exit_tree() -> void:
	AudioManager.stop_bgm()
