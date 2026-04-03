extends Node

var audio_stream_beep: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = _load_audio_stream(&"res://audios/beep.wav")
		return audio_stream_beep

var audio_stream_beep2: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = _load_audio_stream(&"res://audios/beep2.mp3")
		return audio_stream_beep

var audio_selection: AudioStream:
	get:
		if not audio_stream_beep:
			audio_stream_beep = _load_audio_stream(&"res://audios/selection.wav")
		return audio_stream_beep

func _load_audio_stream(path: StringName) -> AudioStream:
	var stream: AudioStream = ResourceLoader.load(path)
	if not stream:
		stream = ResourceLoader.load(&"res://audios/beep.wav")
		push_error(path + " missing")
	return stream
