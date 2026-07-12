extends Button
class_name GameButton

func _ready() -> void:
	self.mouse_entered.connect(on_mouse_entered)
	self.focus_entered.connect(update_icon)
	self.focus_exited.connect(update_icon)
	self.button_down.connect(update_icon)
	self.button_up.connect(update_icon)
	update_icon()

func on_mouse_entered() -> void:
	grab_focus()

func update_icon() -> void:
	var image = ResourceLoader.load("uid://dkajou5ypu7d0") as Texture2D
	icon = AtlasTexture.new()
	icon.atlas = image
	assert(image)
	if has_focus():
		if button_pressed:
			icon.region = Rect2(10, 1, 9, 8)
		else:
			icon.region = Rect2(11, 1, 9, 8)
	else:
		icon.region = Rect2(1, 1, 9, 8)
