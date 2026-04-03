extends Button
class_name CustomButton

func _ready() -> void:
	connect("mouse_entered", _on_mouse_entered)
	connect("mouse_exited", _on_mouse_exited)
	connect("focus_entered", _on_focus_entered)

func _on_mouse_entered() -> void:
	grab_focus()

func _on_mouse_exited() -> void:
	release_focus()

func _on_focus_entered() -> void:
	AudioManager.play(Resources.audio_stream_beep)
