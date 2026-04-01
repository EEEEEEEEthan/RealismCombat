extends Button
class_name CustomButton

var _player: AudioStreamPlayer:
	get:
		if not _player:
			_player = AudioStreamPlayer.new()
			_player.stream = ResourceLoader.load("res://beep.mp3")
			if not _player.stream:
				push_error("audio missing: beep");
			add_child(_player)
		return _player

func _ready() -> void:
	connect("mouse_entered", _on_mouse_entered)
	connect("mouse_exited", _on_mouse_exited)
	connect("focus_entered", _on_focus_entered)

func _on_mouse_entered() -> void:
	grab_focus()

func _on_mouse_exited() -> void:
	release_focus()

func _on_focus_entered() -> void:
	_player.play()
