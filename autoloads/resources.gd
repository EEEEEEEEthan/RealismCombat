extends Node
class_name Resources

static var texture2d_theme_atlas: Texture2D:
	get:
		if not texture2d_theme_atlas: texture2d_theme_atlas = _load_texture2d(&"res://theme_atlas.png")
		return texture2d_theme_atlas

static var atlas_texture_theme_up: AtlasTexture:
	get:
		if not atlas_texture_theme_up:
			atlas_texture_theme_up = AtlasTexture.new()
			atlas_texture_theme_up.atlas = texture2d_theme_atlas
			atlas_texture_theme_up.region = Rect2(21, 2, 8, 5)
		return atlas_texture_theme_up

static func _load_texture2d(path: StringName) -> Texture2D:
	var texture: Texture2D = ResourceLoader.load(path)
	if not texture:
		texture = ResourceLoader.load(&"res://theme_atlas.png")
		push_error("%s missing" % path)
	return texture
