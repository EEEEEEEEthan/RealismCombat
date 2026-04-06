class_name CombatActionError

enum ErrorCode
{
	NONE,
	ABSTRACT_CLASS,
}

var code: ErrorCode
var message: String

func _init(error_code:ErrorCode, error_message:String = &"") -> void:
	code = error_code
	message = error_message
