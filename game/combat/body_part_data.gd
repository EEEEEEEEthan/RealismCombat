class_name BodyPartData

signal hp_changed

var part: Defs.BodyPart = Defs.BodyPart.HEAD
var hp: int:
	set(v):
		hp = v
		hp_changed.emit()

var hp_max: int
