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
		if slash > 0:
			_string += str(slash) + &"砍"
		if pierce > 0:
			_string += str(pierce) + &"刺"
		if blunt > 0:
			_string += str(blunt) + &"钝"
		if not _string:
			_string = "0"
	return _string
