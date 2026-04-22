extends RefCounted
class_name GameVersion

## 与 semver 对齐的第三段，此处为 16 位
var major: int
var minor: int
var patch: int
var build: int

const CURRENT_MAJOR: int = 0
const CURRENT_MINOR: int = 0
const CURRENT_PATCH: int = 0
const CURRENT_BUILD: int = 0

static var CURRENT: GameVersion = GameVersion.new(
	CURRENT_MAJOR, CURRENT_MINOR, CURRENT_PATCH, CURRENT_BUILD,
)

func _init(major, minor, patch, build) -> void:
	self.major = major
	self.minor = minor
	self.patch = patch
	self.build = build

func write_to_file(file_access: FileAccess) -> void:
	file_access.store_8(major)
	file_access.store_8(minor)
	file_access.store_16(patch)
	file_access.store_32(build)

static func read_from_file(file_access: FileAccess) -> GameVersion:
	return GameVersion.new(
		file_access.get_8(),
		file_access.get_8(),
		file_access.get_16(),
		file_access.get_32(),
	)

func to_display_string() -> String:
	return "%d.%d.%d" % [major, minor, patch] + " build %d" % build
