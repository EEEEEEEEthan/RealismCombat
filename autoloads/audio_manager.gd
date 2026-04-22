extends Node

var _bgm_player: AudioStreamPlayer:
	get:
		if not _bgm_player:
			_bgm_player = AudioStreamPlayer.new()
			add_child(_bgm_player)
		return _bgm_player

func play_turn_begin():
	play_sound_effect(Resources.audio_stream_start)

func play_hit():
	play_sound_effect(Resources.audio_stream_hit)

func play_dodge():
	play_sound_effect(Resources.audio_stream_dodge)

func play_menu_bgm():
	_play_bgm(Resources.audio_stream_menu_music)

func stop_bgm():
	_bgm_player.stream = null

func play_sound_effect(stream: AudioStream) -> void:
	var player = AudioStreamPlayer.new()
	add_child(player)
	player.stream = stream
	player.play()
	await player.finished
	player.queue_free()

func play_background_music(stream: AudioStream) -> void:
	_bgm_player.stream = stream
	_bgm_player.play()

func _play_bgm(stream: AudioStream) -> void:
	_bgm_player.stream = stream
	_bgm_player.play()
