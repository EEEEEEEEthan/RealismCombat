extends Node

var _players: Array = []

var _player: AudioStreamPlayer:
	get:
		for player: AudioStreamPlayer in _players:
			if player.finished:
				return player
		var player = AudioStreamPlayer.new()
		_players.append(player)
		add_child(player)
		return player

func play_beep():
	_play(Resources.audio_stream_beep)

func play_selection():
	_play(Resources.audio_stream_selection)

func _play(stream: AudioStream) -> void:
	_player.stream = stream
	_player.play()
