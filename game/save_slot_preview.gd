extends RefCounted
class_name SaveSlotPreview

## 无文件；此时 snapshot 为 null
var is_empty: bool
## 有档且可解析的存档头；有档但损坏为 null。与 is_empty 区分无档/损坏
var snapshot: SaveSnapshot
