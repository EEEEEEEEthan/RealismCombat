class_name Damage

var slash: int
var pierce: int
var blunt: int
var sum: int:
	get: return slash + pierce + blunt

var _string: String

func _init(s: int, p: int, b: int) -> void:
	slash = s
	pierce = p
	blunt = b

func reduced_by(protection: Protection) -> Damage:
	return Damage.new(
		maxi(slash - protection.slash, 0),
		maxi(pierce - protection.pierce, 0),
		maxi(blunt - protection.blunt, 0),
	)

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
