extends Node
class_name Game

var combat: Combat
@onready var character_ethan: Character = %Ethan

func _create_character(character_name: String) -> Character:
	var chr:Character = %CharacterTemplate.create_instance()
	chr.name = character_name
	chr.character_name = character_name
	return chr

func _ready() -> void:
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
		combat = %Combat.create_instance()
		combat.add_character(character_ethan, 0)
		var dove:Character = _create_character("Dove")
		combat.add_character(dove, 1)
		combat.run()
	elif index == 6:
		queue_free()
