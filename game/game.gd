extends Node
class_name Game

var combat: Combat
var character_ethan: Character
var inventory: Inventory


func _create_character(character_name: String) -> Character:
	return Character.new(self, character_name)


func _ready() -> void:
	AudioManager.play_menu_bgm()
	character_ethan = Character.new(self, "Ethan")
	inventory = Inventory.new()
	var starting_sword := ShortSword.new()
	starting_sword.quality = PropertyInt.new(4, 4)
	inventory.add_item(starting_sword)
	var main_menu_choice := -1
	while true:
		var menu = Dialogues.create_menu_dialogue()
		menu.title = "Realism Combat"
		menu.options = [
			MenuItemData.new("测试项", false, "测试项"),
			MenuItemData.new("物品栏", false, "查看持有的道具"),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new(),
			MenuItemData.new("返回菜单", false, "离开"),
		] as Array[MenuItemData]
		main_menu_choice = await menu.pressed
		menu.queue_free()
		if main_menu_choice == 6:
			queue_free()
			return
		if main_menu_choice == 1:
			await _run_inventory_menu()
			continue
		if main_menu_choice == 0:
			break
	combat = %Combat.create_instance()
	combat.add_character(character_ethan, 0)
	var dove: Character = _create_character("Dove")
	combat.add_character(dove, 1)
	combat.run()


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
