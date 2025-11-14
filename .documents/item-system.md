# 物品系统

## 概述

物品系统管理游戏中的装备实体，包括物品的定义、装备槽的管理、物品与身体部位的关联，以及在战斗中的应用。

## 核心类

### Item 抽象类

`Item` 是游戏中所有装备的基类，位于 `Scripts/Items/Item.cs`：

- 实现 `ICombatTarget` 接口，可在战斗流程中作为目标被选择或结算伤害
- 实现 `IItemContainer` 接口，支持嵌套装备（如武器上的宝石）
- 包含 `ItemIdCode id`：物品的唯一标识
- 包含 `ItemFlagCode flag`：物品类型标志，用于匹配装备槽
- 包含 `PropertyInt HitPoint`：装备的耐久属性，当耐久为 0 时 `Available` 返回 `false`
- 包含 `ItemSlot[] Slots`：支持装备嵌套的槽位数组
- 提供抽象的 `Name` 属性，子类需要实现显示名称

#### 序列化

- `Item.Load(BinaryReader reader)`：静态工厂方法，根据 `ItemIdCode` 反序列化对应的物品子类
- `Serialize(BinaryWriter writer)`：序列化物品数据，包括 ID、槽位和子类特定数据
- 子类需要实现 `OnSerialize` 和 `OnDeserialize` 来处理特定数据

#### 实现示例

`LongSword` 是当前唯一的物品实现：
- 继承自 `Item`，使用 `ItemIdCode.LongSword` 标识
- 标志为 `ItemFlagCode.Arm`，可装备到手臂槽位
- 显示名称为"长剑"
- 默认耐久为 10/10
- 无额外槽位，`Slots` 为空数组

### ItemSlot 类

`ItemSlot` 管理单个装备槽位，位于 `Scripts/Items/ItemSlot.cs`：

- 构造函数接受 `ItemFlagCode flag` 参数，定义槽位可接受的装备类型
- `Item` 属性用于存取装备，设置时会验证装备标志是否匹配
- 如果装备标志与槽位标志不匹配，会抛出 `ArgumentException`
- 支持二进制序列化，使用 `ReadScope()`、`WriteScope()` 保证数据块完整性

### ItemFlagCode 枚举

`ItemFlagCode` 是标志枚举，位于 `Scripts/Items/ItemFlagCode.cs`：

- 使用 `[Flags]` 特性，支持组合标志
- `Arm = 1 << 0`：表示武器类型装备
- 用于匹配物品与装备槽位，只有标志匹配的物品才能放入对应槽位

### ItemIdCode 枚举

`ItemIdCode` 定义物品的唯一标识，位于 `Scripts/Items/Item.cs`：

- 当前包含 `LongSword`：长剑
- 用于序列化时识别物品类型，反序列化时根据 ID 创建对应的子类实例

### IItemContainer 接口

`IItemContainer` 接口定义物品容器，位于 `Scripts/Items/IItemContainer.cs`：

- 包含 `ItemSlot[] Slots` 属性
- 由 `Item` 和 `BodyPart` 实现，支持装备嵌套和身体部位装备管理

## 与战斗系统的集成

### 装备挂载

- `BodyPart` 通过 `Slots` 数组管理装备槽位
- 默认情况下，双臂部位（`LeftArm`、`RightArm`）拥有 `ItemFlagCode.Arm` 类型的槽位
- 装备通过设置 `bodyPart.Slots[0].Item = item` 进行挂载

### 战斗目标

- `Item` 实现 `ICombatTarget` 接口，可作为战斗中的攻击目标
- 在格挡选择时，装备会与身体部位一起出现在可选目标列表中
- `GetBlockTargets()` 方法会返回角色的所有可用身体部位和装备

### 伤害结算

- 装备拥有独立的耐久属性 `HitPoint`
- 当装备受到伤害时，耐久会减少
- 当耐久为 0 时，装备的 `Available` 属性返回 `false`，不再可用

## 序列化与存档

### 序列化格式

物品序列化使用 `BinaryReaderWriterExtensions` 提供的作用域机制：
- 使用 `ReadScope()`、`WriteScope()` 保证数据块长度安全
- 序列化顺序：ID（`ulong`）→ 槽位数量（`int`）→ 各槽位数据 → 子类特定数据

### 兼容性处理

- 反序列化时会检查槽位数量，如果存档中的数量与当前版本不一致，会安全地忽略多余的槽位或跳过缺失的槽位
- 保证存档在不同版本间的兼容性

## 扩展指南

### 添加新物品

1. 在 `ItemIdCode` 枚举中添加新标识
2. 创建继承自 `Item` 的子类
3. 在 `Item.Load()` 的 switch 语句中添加新物品的创建逻辑
4. 实现 `Name` 属性返回显示名称
5. 实现 `OnSerialize` 和 `OnDeserialize` 处理特定数据（如有）

### 添加新装备类型

1. 在 `ItemFlagCode` 枚举中添加新标志（如 `Shield = 1 << 1`）
2. 在需要装备该类型物品的身体部位上创建对应标志的 `ItemSlot`
3. 创建使用该标志的物品类

### 装备嵌套

- `Item` 包含 `Slots` 数组，支持在装备上再装备其他物品
- 通过 `IItemContainer` 接口统一管理
- 当前实现中暂未使用，但架构已支持

