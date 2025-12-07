# 存档系统

## 概述

- 项目提供 7 个存档槽位，路径格式为 `user://save_slot_{n}.sav`，由 `ProgramRoot` 负责创建目录与构建菜单。
- 读取或展示存档槽时会尝试解析快照信息；解析失败会记录日志并将槽位视为“空”。
- 存档菜单在新建/读取时复用同一套 UI 逻辑，支持覆盖确认与无存档提示。

## Snapshot 结构

- 位置：`Scripts/Snapshot.cs`，用于记录存档元数据。
- 字段：
  - `GameVersion version`：存档版本，保存时使用 `GameVersion.newest`。
  - `DateTime savedAt`：保存时间（本地时区）。
  - `ScriptCode scriptIndex`：当前剧本进度，用于恢复游戏场景。
- 属性：
  - `Title`：根据当前时间与 `savedAt` 的差值生成相对时间描述（刚刚/分钟前/小时前/天前/周前/月前/日期）。
  - `Desc`：当前版本号字符串。
- 构造：
  - `Snapshot(BinaryReader reader)`：从存档读取快照。
  - `Snapshot(Game game)`：从运行时状态生成快照（捕获 `ScriptIndex` 与当前时间）。

## 序列化格式

- 使用 `BinaryReaderWriterExtensions` 的作用域机制包裹数据块，保证长度对齐。
- 顺序（作用域内）：
  1. `GameVersion.Serialize(reader|writer)`
  2. `savedAt`：以 UTC ticks 存储，读取时转为本地时间
  3. `scriptIndex`：`int`
- 扩展字段时应继续置于作用域内，确保旧版本能安全跳过未知数据。

## 存档读写流程

- 保存：`Game.Save()` 在手动存档或退出游戏时调用，依次写入 `Snapshot`、玩家列表、`ScriptIndex`。会自动创建存档目录。
- 读取：`Game` 的读取构造函数先解析 `Snapshot`，再读取玩家数据和剧本索引后进入游戏循环。
- 槽位展示：`ProgramRoot.CreateSaveSlotOption()` 读取快照并使用 `Title`/`Desc` 填充菜单文案，异常时记录日志并回退为空槽描述。

## 兼容性与扩展

- 新增快照字段时保持写入顺序，并通过 `GameVersion` 协调版本演进。
- 读取失败的存档会在菜单阶段就抛出并记录，不会进入游戏主循环；调整存档格式时需同步更新解析逻辑。


