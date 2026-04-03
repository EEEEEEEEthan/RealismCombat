extends Node

var audio_stream_beep: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = ResourceLoader.load(&"res://audios/beep.wav")
			if not audio_stream_beep:
				push_error(&"res://audios/beep.wav missing")
		return audio_stream_beep


var audio_stream_beep2: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = ResourceLoader.load(&"res://audios/beep2.mp3")
			if not audio_stream_beep:
				push_error(&"res://audios/beep2.mp3 missing")
		return audio_stream_beep
