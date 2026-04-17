@abstract
class_name Item

func get_protection() -> Protection:
	push_error("抽象基类")
	return Protection.new(0, 0, 0)
