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

var audio_stream_selection: AudioStream:
	get:
		if not audio_stream_selection:
			audio_stream_selection = _load_audio_stream(&"res://audios/selection.wav")
		return audio_stream_selection

var texture2d_theme_atlas: Texture2D:
	get:
		if not texture2d_theme_atlas:
			texture2d_theme_atlas = _load_texture2d(&"res://textures/theme_atlas.png")
		return texture2d_theme_atlas

var atlas_texture_theme_right: AtlasTexture:
	get:
		if not atlas_texture_theme_right:
			atlas_texture_theme_right = AtlasTexture.new()
			atlas_texture_theme_right.atlas = texture2d_theme_atlas
			atlas_texture_theme_right.region = Rect2(11, 1, 8, 8)
		return atlas_texture_theme_right

var atlas_texture_theme_up: AtlasTexture:
	get:
		if not atlas_texture_theme_up:
			atlas_texture_theme_up = AtlasTexture.new()
			atlas_texture_theme_up.atlas = texture2d_theme_atlas
			atlas_texture_theme_up.region = Rect2(21, 2, 8, 5)
		return atlas_texture_theme_up

func _load_audio_stream(path: StringName) -> AudioStream:
	var stream: AudioStream = ResourceLoader.load(path)
	if not stream:
		stream = ResourceLoader.load(&"res://audios/beep.wav")
		push_error(path + " missing")
	return stream

func _load_texture2d(path: StringName) -> Texture2D:
	var texture: Texture2D = ResourceLoader.load(path)
	if not texture:
		texture = ResourceLoader.load(&"res://textures/theme_atlas.png")
		push_error(path + " missing")
	return texture
