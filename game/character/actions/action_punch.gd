extends Action

func static_valid_from_body(from_body: BodyPart) -> Outcome:
	if not from_body.is_hand:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_from_body(from_body: BodyPart) -> Outcome:
	if from_body.hp.value <= 0:
		return Outcome.from_failure(from_body.part_name + &"无法行动")
	return Outcome.from_success()

func static_valid_to_body(to_body: BodyPart) -> Outcome:
	if not to_body.character.alive:
		return Outcome.from_failure()
	return Outcome.from_success()

func dynamic_valid_to_body(to_body: BodyPart) -> Outcome:
	if to_body.hp.value <= 0:
		return Outcome.from_failure(to_body.part_name + &"早已无法行动")
	return Outcome.from_success()
