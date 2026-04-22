extends RefCounted
class_name SaveSnapshot

## 存档文件头魔数字符串（全 ASCII，UTF-8 字节数等于 length）
const _FILE_MAGIC: String = "e4b8912a-7c3f-4d6e-9b21-4f8a2c1d0e5b"

var game_version: GameVersion
## 主菜单/列表展示用名称（例如领队角色名）
var save_name: String
## 存档时 Unix 秒
var saved_at_unix: float

static func read_header_including_magic(file_access: FileAccess) -> SaveSnapshot:
	return _read_header_from_file(file_access, true)

## 在魔数+快照头之后是角色区（先存魔数+版本+时间+名称，再 [method Game.save_game] 中写队伍）
static func write_to_file(
	file_access: FileAccess, snapshot: SaveSnapshot,
) -> void:
	_write_magic(file_access)
	snapshot.game_version.write_to_file(file_access)
	file_access.store_64(int(snapshot.saved_at_unix))
	file_access.store_pascal_string(snapshot.save_name)

static func preview_path(path: String) -> SaveSlotPreview:
	var preview: SaveSlotPreview = SaveSlotPreview.new()
	if not FileAccess.file_exists(path):
		preview.is_empty = true
		return preview
	var file_access: FileAccess = FileAccess.open(path, FileAccess.READ)
	if file_access == null:
		return preview
	var read_snapshot: SaveSnapshot = read_header_including_magic(file_access)
	file_access.close()
	if read_snapshot == null:
		return preview
	preview.snapshot = read_snapshot
	return preview

static func format_time_ago(saved_at_seconds: int) -> String:
	var now_s: float = int(Time.get_unix_time_from_system())
	var delta: float = now_s - saved_at_seconds
	if delta < 0:
		return "未来"
	if delta < 60:
		return "刚刚"
	if delta < 3600:
		return "%d 分钟前" % (delta / 60)
	if delta < 86400:
		return "%d 小时前" % (delta / 3600)
	if delta < 86400 * 7:
		return "%d 天前" % (delta / 86400)
	if delta < 86400 * 30:
		return "%d 周前" % (delta / 86400 / 7)
	if delta < 86400 * 365:
		return "%d 个月前" % (delta / 86400 / 30)
	return "%d 年前" % (delta / 86400 / 365)

static func _read_header_from_file(
	file_access: FileAccess, need_magic: bool,
) -> SaveSnapshot:
	if need_magic and not _read_magic(file_access):
		return null
	var version: GameVersion = GameVersion.read_from_file(file_access)
	var at_s: int = int(file_access.get_64())
	var s_name: String = file_access.get_pascal_string()
	return SaveSnapshot.new(version, s_name, at_s)

static func _write_magic(file_access: FileAccess) -> void:
	file_access.store_buffer(_FILE_MAGIC.to_utf8_buffer())

static func _read_magic(file_access: FileAccess) -> bool:
	var need_len: int = _FILE_MAGIC.length()
	if file_access.get_length() < file_access.get_position() + need_len:
		return false
	var buffer: PackedByteArray = file_access.get_buffer(need_len)
	return buffer.get_string_from_utf8() == _FILE_MAGIC

func _init(game_v: GameVersion, name: String, at_unix: float) -> void:
	self.game_version = game_v
	self.save_name = name
	self.saved_at_unix = at_unix
