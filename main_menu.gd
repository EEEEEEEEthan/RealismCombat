extends Node
class_name MainMenu

func _ready() -> void:
	var program: Program = get_parent()
	var menu:MenuDialogue = Dialogues.create_menu()
	menu.title = "Realism Combat"
	menu.options = [
		MenuItemData.new("新游戏", false, "开启一段新的故事"),
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
		program.create_new_game()
