# 版本系统

## 概述

版本系统定义了游戏存档的版本号格式，用于标识存档的版本，支持版本比较和序列化。

## 核心结构

### GameVersion 结构体

`GameVersion` 是只读结构体，位于 `Scripts/GameVersion.cs`，用于表示游戏版本号：

#### 版本格式

版本号采用三段式格式：`Major.Minor.Build`
- `Major`：主版本号（`ushort`，16 位）
- `Minor`：次版本号（`ushort`，16 位）
- `Build`：构建号（`uint`，32 位）

#### 存储方式

版本号内部存储为单个 `ulong`（64 位）值：
- 高位 16 位：`Major`
- 中间 16 位：`Minor`
- 低位 32 位：`Build`

#### 属性

- `ushort Major`：主版本号
- `ushort Minor`：次版本号
- `uint Build`：构建号

#### 构造方法

- `GameVersion(ushort major, ushort minor, uint build)`：使用指定的版本号创建版本
- `GameVersion(BinaryReader reader)`：从二进制读取器反序列化版本
- `GameVersion(ulong value)`：使用内部值创建版本（私有）

#### 常量

- `GameVersion.newest`：最新版本，值为 `0.0.0`

#### 比较操作符

支持完整的比较操作符：
- `==`、`!=`：相等性比较
- `<`、`>`、`<=`、`>=`：大小比较

比较基于内部 `ulong` 值，主版本号优先级最高，其次是次版本号，最后是构建号。

#### 序列化方法

- `Serialize(BinaryWriter writer)`：将版本序列化到二进制写入器，写入单个 `ulong` 值

#### 其他方法

- `ToString()`：返回格式化的版本字符串（`"Major.Minor.Build"`）
- `Equals(object? obj)`：对象相等性比较
- `Equals(GameVersion other)`：版本相等性比较
- `GetHashCode()`：返回哈希码

## 使用场景

### 存档版本标识

- `Game.Snapshot` 使用 `GameVersion` 存储存档版本
- 序列化时写入版本号，反序列化时读取版本号
- 用于兼容性检查和版本迁移

### 版本比较

```csharp
// 检查存档版本是否是最新版本
if (snapshot.Version == GameVersion.newest)
{
    // 最新版本，无需迁移
}

// 比较版本
if (snapshot.Version < GameVersion.newest)
{
    // 旧版本，需要迁移
}
```

### 序列化与反序列化

```csharp
// 序列化
var version = new GameVersion(1, 2, 345);
using var stream = new FileStream("version.dat", FileMode.Create);
using var writer = new BinaryWriter(stream);
version.Serialize(writer);

// 反序列化
using var stream = new FileStream("version.dat", FileMode.Open);
using var reader = new BinaryReader(stream);
var version = new GameVersion(reader);
```

## 序列化格式

### 版本序列化

1. 写入单个 `ulong` 值
2. 该值包含完整的版本信息（Major、Minor、Build）

## 设计考虑

### 为什么使用 ulong

- 使用单个 `ulong` 值存储版本号，便于比较和序列化
- 位操作高效，比较操作直接比较数值
- 序列化格式简单，只需写入一个 8 字节值

### 版本号范围

- `Major` 和 `Minor` 使用 `ushort`（0-65535）
- `Build` 使用 `uint`（0-4294967295）
- 对于游戏版本来说，这个范围已经足够

### 版本比较

- 使用数值比较，主版本号优先级最高
- 例如：`1.2.3 < 2.0.0`，`1.2.3 < 1.3.0`，`1.2.3 < 1.2.4`

### 最新版本

- `GameVersion.newest` 定义为 `0.0.0`
- 这是默认版本，表示当前最新版本
- 如果版本号格式改变，可以调整此常量

## 在存档系统中的应用

### Game.Snapshot

`Game.Snapshot` 使用 `GameVersion` 存储存档版本：

```csharp
public record Snapshot
{
    readonly GameVersion version;
    public GameVersion Version => version;
    // ...
}
```

- 创建新存档时，使用 `GameVersion.newest`
- 序列化时写入版本号
- 反序列化时读取版本号，可用于兼容性检查

### 版本兼容性

- 当前实现中，反序列化时读取版本号但不进行兼容性检查
- 可以通过版本比较实现版本迁移逻辑
- 例如：如果版本低于某个阈值，执行数据迁移

## 扩展指南

### 添加版本迁移

1. 在反序列化存档时，检查版本号
2. 如果版本低于目标版本，执行迁移逻辑
3. 更新版本号为最新版本

### 修改版本格式

- 如果版本格式需要改变，可以创建新的版本结构
- 需要处理旧版本的兼容性
- 可以通过版本号判断使用哪个版本结构

### 添加版本元数据

- 可以扩展 `GameVersion` 添加更多信息（如构建日期、分支名称等）
- 需要修改序列化格式以支持新字段

