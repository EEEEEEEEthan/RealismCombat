extends RefCounted
class_name SaveSlotPreview

## 无文件
var is_empty: bool
## 存在但头或内容无法解析
var is_corrupt: bool
## 有快照且未损坏时有效
var snapshot: SaveSnapshot
