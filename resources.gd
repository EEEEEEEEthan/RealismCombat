extends Node

var audio_stream_beep: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = ResourceLoader.load("res://audios/beep.mp3")
			if not audio_stream_beep:
				push_error("res://audios/beep.mp3 missing")
		return audio_stream_beep
