class_name Outcome

static var _success:Outcome:
	get:
		if not _success:
			_success = Outcome.new()
			_success.success = true
		return _success

var success: bool
var error_message: String = ""

static func from_success() -> Outcome:
	return _success

static func from_failure(failure_reason: String = &"") -> Outcome:
	var outcome := Outcome.new()
	outcome.success = false
	outcome.error_message = failure_reason
	return outcome
