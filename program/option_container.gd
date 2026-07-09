@tool
extends VBoxContainer
class_name OptionContainer

var _up_arrow: AtlasTexture:
	get:
		if not _up_arrow:
			var image = ResourceLoader.load("uid://dkajou5ypu7d0") as Image
			_up_arrow = AtlasTexture.new()
			_up_arrow.atlas = image
			_up_arrow.region = Rect2(21, 2, 8, 5)
		return _up_arrow

func _init() -> void:
	var up_arrow = TextureRect.new()
	up_arrow.texture = up_arrow
	up_arrow.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	up_arrow.custom_minimum_size = Vector2(0, 8)
	add_child(up_arrow, false, Node.INTERNAL_MODE_FRONT)
	var down_arrow = TextureRect.new()
	down_arrow.texture = up_arrow
	down_arrow.stretch_mode = TextureRect.STRETCH_KEEP_CENTERED
	down_arrow.custom_minimum_size = Vector2(0, 8)
	down_arrow.flip_v = true
	add_child(down_arrow, false, Node.INTERNAL_MODE_BACK)

func _exit_tree() -> void:
	_up_arrow = null
