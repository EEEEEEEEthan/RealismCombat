extends Node
class_name Resources

static var audio_stream_beep: AudioStream:
	get:
		if not audio_stream_beep: audio_stream_beep = _load_audio_stream(&"res://audios/beep.wav")
		return audio_stream_beep

static var audio_stream_double_beep: AudioStream:
	get:
		if not audio_stream_double_beep: audio_stream_double_beep = _load_audio_stream(&"res://audios/double_beep.wav")
		return audio_stream_double_beep

static var audio_stream_selection: AudioStream:
	get:
		if not audio_stream_selection: audio_stream_selection = _load_audio_stream(&"res://audios/selection.wav")
		return audio_stream_selection

static var audio_stream_start: AudioStream:
	get:
		if not audio_stream_start: audio_stream_start = _load_audio_stream(&"res://audios/start.wav")
		return audio_stream_start

static var audio_stream_hit: AudioStream:
	get:
		if not audio_stream_hit: audio_stream_hit = _load_audio_stream(&"res://audios/hit.wav")
		return audio_stream_hit

static var audio_stream_dodge: AudioStream:
	get:
		if not audio_stream_dodge: audio_stream_dodge = _load_audio_stream(&"res://audios/dodge.wav")
		return audio_stream_dodge

static var audio_stream_menu_music: AudioStream:
	get:
		if not audio_stream_menu_music: audio_stream_menu_music = _load_audio_stream(&"res://audios/menu_music.wav")
		return audio_stream_menu_music

static var audio_stream_battle_music: AudioStream:
	get:
		if not audio_stream_battle_music: audio_stream_battle_music = _load_audio_stream(&"res://audios/battle_music.wav")
		return audio_stream_battle_music

static var texture2d_theme_atlas: Texture2D:
	get:
		if not texture2d_theme_atlas: texture2d_theme_atlas = _load_texture2d(&"res://theme_atlas.png")
		return texture2d_theme_atlas

static var atlas_texture_theme_right: AtlasTexture:
	get:
		if not atlas_texture_theme_right:
			atlas_texture_theme_right = AtlasTexture.new()
			atlas_texture_theme_right.atlas = texture2d_theme_atlas
			atlas_texture_theme_right.region = Rect2(11, 1, 8, 8)
		return atlas_texture_theme_right

static var atlas_texture_theme_up: AtlasTexture:
	get:
		if not atlas_texture_theme_up:
			atlas_texture_theme_up = AtlasTexture.new()
			atlas_texture_theme_up.atlas = texture2d_theme_atlas
			atlas_texture_theme_up.region = Rect2(21, 2, 8, 5)
		return atlas_texture_theme_up

static func _load_audio_stream(path: StringName) -> AudioStream:
	var stream: AudioStream = ResourceLoader.load(path)
	if not stream:
		stream = ResourceLoader.load(&"res://audios/beep.mp3")
		push_error("%s missing" % path)
	return stream

static func _load_texture2d(path: StringName) -> Texture2D:
	var texture: Texture2D = ResourceLoader.load(path)
	if not texture:
		texture = ResourceLoader.load(&"res://theme_atlas.png")
		push_error("%s missing" % path)
	return texture
