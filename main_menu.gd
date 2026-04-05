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
	while true:
		var index = await menu.pressed
		menu.visible = false
		if index == 0:
			var game = program.create_new_game()
			print(game)
			await game.tree_exiting
			menu.visible = true
			menu.grab_focus()
