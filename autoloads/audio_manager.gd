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

func play(stream: AudioStream) -> void:
	_player.stream = stream
	_player.play()
