extends Node
class_name Game

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
		MenuItemData.new("退出", false, "离开"),
	]
	var index = await menu.pressed
	menu.queue_free()
	if index == 0:
		print("asdf")
	elif index == 6:
		var program:Program = get_parent()
		program.show_main_menu()
		queue_free()
