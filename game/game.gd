extends Node
class_name Game

var combat: Combat
var character_ethan: Character
var character_rowan: Character
## 当前玩家方参战角色（顺序即编队顺序）
var player_side_characters: Array[Character] = []
var inventory: Inventory


func _ready() -> void:
	AudioManager.play_background_music(%Audios.menu_music)
	character_ethan = Character.create_default(self, "Ethan")
	character_rowan = Character.create_default(self, "Rowan")
	player_side_characters = [character_ethan, character_rowan]
	inventory = Inventory.new()
	while true:
		var menu = Dialogues.create_menu_dialogue()
		menu.title = "Realism Combat"
		menu.options = [
			MenuItemData.new("测试项..", false, "测试项"),
			MenuItemData.new("装备..", false, "为角色装备或卸下物品"),
			MenuItemData.new("物品栏..", false, "查看持有的道具"),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new("返回菜单", false, "离开"),
		] as Array[MenuItemData]
		var main_menu_choice: int = await menu.pressed
		menu.queue_free()
		if main_menu_choice == 6:
			queue_free()
			return
		if main_menu_choice == 2:
			await _run_inventory_menu()
			continue
		if main_menu_choice == 1:
			await _run_equipment_menu()
			continue
		if main_menu_choice == 0:
			combat = %Combat.create_instance()
			for player_character in player_side_characters:
				combat.add_character(player_character, Combat.PLAYER_SIDE)
			var dove: Character = Character.new(self, "Dove")
			combat.add_character(dove, Combat.ENEMY_SIDE)
			await combat.run()
			combat = null
			continue


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
