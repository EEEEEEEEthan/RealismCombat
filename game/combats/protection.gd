class_name Protection

var slash: int
var pierce: int
var blunt: int

var _string: String

func _init(s: int, p: int, b: int) -> void:
	slash = s
	pierce = p
	blunt = b

func _to_string() -> String:
	if not _string:
		var parts: PackedStringArray = []
		if slash > 0:
			parts.append("%d劈" % slash)
		if pierce > 0:
			parts.append("%d穿" % pierce)
		if blunt > 0:
			parts.append("%d钝" % blunt)
		_string = "".join(parts) if parts.size() else "0"
	return _string
