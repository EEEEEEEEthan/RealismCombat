@tool
extends Texture2D
class_name EditableTexture

@export var _raw_texture: ImageTexture:
	get:
		if not _raw_texture:
			var image := Image.create(16, 16, false, Image.FORMAT_RGBA8);
			image.fill(Color.WHITE)
			_raw_texture = ImageTexture.create_from_image(image);
		return _raw_texture
	set (value):
		_raw_texture = value
		emit_changed()
		notify_property_list_changed()

func _get_width() -> int:
	return _raw_texture.get_width()

func _get_height() -> int:
	return _raw_texture.get_height()

func _get_rid() -> RID:
	return _raw_texture.get_rid()

func _has_alpha() -> bool:
	return _raw_texture.has_alpha()

func _draw(rid: RID, pos: Vector2, modulate: Color, transpose: bool) -> void:
	_raw_texture.draw(rid, pos, modulate, transpose)

func _draw_rect(rid: RID, rect: Rect2, tile: bool, modulate: Color, transpose: bool) -> void:
	_raw_texture.draw_rect(rid, rect, tile, modulate, transpose)

func _draw_rect_region(rid: RID, rect: Rect2, src_rect: Rect2, modulate: Color, transpose: bool, clip_uv: bool) -> void:
	_raw_texture.draw_rect_region(rid, rect, src_rect, modulate, transpose, clip_uv)
