extends Node
class_name Game

var combat: Combat
var character_ethan: Character

func _create_character(character_name: String) -> Character:
	return Character.new(self, character_name)

func _ready() -> void:
	AudioManager.play_menu_bgm()
	character_ethan = Character.new(self, "Ethan")
	var menu = Dialogues.create_menu_dialogue()
	menu.title = "Realism Combat"
	menu.options = [
		MenuItemData.new("测试项", false, "测试项"),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new(),
		MenuItemData.new("返回菜单", false, "离开"),
	] as Array[MenuItemData]
	var index = await menu.pressed
	menu.queue_free()
	if index == 0:
		combat = %Combat.create_instance()
		combat.add_character(character_ethan, 0)
		var dove: Character = _create_character("Dove")
		combat.add_character(dove, 1)
		combat.run()
	elif index == 6:
		queue_free()

func _exit_tree() -> void:
	AudioManager.stop_bgm()
