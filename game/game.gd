extends Node
class_name Game

var character_ethan: Character

func _create_character(character_name: String) -> Character:
	var chr:Character = %CharacterTemplate.create_instance()
	chr.name = character_name
	chr.character_name = character_name
	return chr

func _ready() -> void:
	character_ethan = _create_character("Ethan")
	var menu:MenuDialogue = Dialogues.create_menu()
	menu.title = "Realism Combat"
	menu.options = [
		MenuItemData.new("测试项", false, "测试项"),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new("返回菜单", false, "离开"),
	]
	var index = await menu.pressed
	menu.queue_free()
	if index == 0:
		var combat:Combat = %Combat.create_instance()
		combat.add_character(character_ethan, 0)
		var dove:Character = _create_character("Dove")
		combat.add_character(dove, 1)
		combat.run()
	elif index == 6:
		queue_free()
