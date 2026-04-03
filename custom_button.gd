extends Button
class_name CustomButton

func _ready() -> void:
	connect(&"mouse_entered", _on_mouse_entered)
	connect(&"focus_entered", _on_focus_entered)
	connect(&"pressed", _on_pressed)

func _on_mouse_entered() -> void:
	grab_focus()

func _on_focus_entered() -> void:
	AudioManager.play_beep()

func _on_pressed() -> void:
	AudioManager.play_selection()
	grab_focus()
