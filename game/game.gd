extends Node
class_name Game

var character_ethan: Character

func _ready() -> void:
	character_ethan = Character.create_default("Ethan")
	var menu:MenuDialogue = Dialogues.create_menu()
	menu.title = "Realism Combat"
	menu.options = [
		MenuItemData.new("测试项", false, "测试项"),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new("退出", false, "离开"),
	]
	var index = await menu.pressed
	menu.queue_free()
	if index == 0:
		var combat = Combat.new()
		combat.add_character(character_ethan, 0)
		combat.add_character(Character.create_default("Dove"), 1)
		combat.run()
	elif index == 6:
		var program:Program = get_parent()
		queue_free()
