extends Node

func play_button_hover():
	_play(Resources.audio_stream_beep)

func play_button_press():
	_play(Resources.audio_stream_selection)

func _play(stream: AudioStream) -> void:
	var player = AudioStreamPlayer.new()
	add_child(player)
	player.stream = stream
	player.play()
	await player.finished
	player.queue_free()
