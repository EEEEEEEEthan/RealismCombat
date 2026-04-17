extends Node

var __bgm_player: AudioStreamPlayer:
	get:
		if not __bgm_player:
			__bgm_player = AudioStreamPlayer.new()
			add_child(__bgm_player)
		return __bgm_player

func play_button_hover():
	_play(Resources.audio_stream_beep)

func play_button_press():
	_play(Resources.audio_stream_selection)

func play_turn_begin():
	_play(Resources.audio_stream_start)

func play_hit():
	_play(Resources.audio_stream_hit)

func play_dodge():
	_play(Resources.audio_stream_dodge)

func play_menu_bgm():
	__play_bgm(Resources.audio_stream_menu_music)

func play_battle_bgm():
	__play_bgm(Resources.audio_stream_battle_music)

func stop_bgm():
	__bgm_player.stream = null

func _play(stream: AudioStream) -> void:
	var player = AudioStreamPlayer.new()
	add_child(player)
	player.stream = stream
	player.play()
	await player.finished
	player.queue_free()

func __play_bgm(stream: AudioStream) -> void:
	__bgm_player.stream = stream
	__bgm_player.play()
