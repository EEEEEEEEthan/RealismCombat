# 角色系统

## 概述

角色系统定义了游戏中角色实体及其身体部位，包括角色的属性、身体部位的管理、装备挂载，以及角色在战斗中的应用。

## 核心类

### Character 类

`Character` 是游戏中的角色实体，位于 `Scripts/Characters/Character.cs`：

#### 属性

- `string name`：角色名称
- `PropertyInt speed`：速度属性，决定每帧行动点回复量，默认最大值为 5
- `PropertyDouble actionPoint`：行动点属性，记录当前与最大行动点，默认上限为 10
- `int reaction`：反应点数，用于格挡或闪避，默认值为 1，每回合开始时重置
- `CombatAction? combatAction`：当前执行中的战斗行为，为 `null` 时表示无行动中

#### 身体部位

角色包含六个 `BodyPart` 实例：
- `head`：头部（`BodyPartCode.Head`），无装备槽位
- `leftArm`：左臂（`BodyPartCode.LeftArm`），拥有 `ItemFlagCode.Arm` 类型装备槽位
- `rightArm`：右臂（`BodyPartCode.RightArm`），拥有 `ItemFlagCode.Arm` 类型装备槽位
- `torso`：躯干（`BodyPartCode.Torso`），无装备槽位
- `leftLeg`：左腿（`BodyPartCode.LeftLeg`），无装备槽位
- `rightLeg`：右腿（`BodyPartCode.RightLeg`），无装备槽位

所有身体部位统一暴露在 `IReadOnlyList<BodyPart> bodyParts` 列表中。

#### 存活判断

- `bool IsAlive`：角色被视为存活的条件是头部与躯干仍可用
- 当 `head.Available && torso.Available` 为 `true` 时角色存活

#### 构造与序列化

- `Character(string name)`：创建新角色，初始化所有属性为默认值
- `Character(BinaryReader reader)`：从存档读取角色数据
- `Serialize(BinaryWriter writer)`：将角色数据写入存档

序列化格式使用 `ReadScope()`、`WriteScope()` 保证数据块完整性，顺序为：名称 → 速度属性 → 行动点属性 → 各身体部位数据。

### BodyPart 类

`BodyPart` 表示角色的身体部位，位于 `Scripts/Characters/BodyPart.cs`：

#### 属性

- `BodyPartCode id`：身体部位的标识
- `PropertyInt HitPoint`：生命值属性，不同部位的最大值不同：
  - 头部（`Head`）：最大值为 5
  - 左臂、右臂（`LeftArm`、`RightArm`）：最大值为 8
  - 左腿、右腿（`LeftLeg`、`RightLeg`）：最大值为 8
  - 躯干（`Torso`）：最大值为 10
- `bool Available`：部位是否仍可用，当 `HitPoint.value > 0` 时返回 `true`
- `ItemSlot[] Slots`：装备槽位数组，管理该部位可装备的物品
- `string Name`：部位在日志或界面上的显示名称，通过扩展方法 `GetName()` 获取

#### 接口实现

- `ICombatTarget`：身体部位可作为战斗中的攻击目标
- `IItemContainer`：身体部位是物品容器，可挂载装备

#### 扩展方法

`BodyPartExtensions.GetName(this BodyPart bodyPart)` 提供中文显示名称：
- `Head` → "头部"
- `LeftArm` → "左臂"
- `RightArm` → "右臂"
- `Torso` → "躯干"
- `LeftLeg` → "左腿"
- `RightLeg` → "右腿"

### BodyPartCode 枚举

`BodyPartCode` 定义身体部位的标识，位于 `Scripts/Characters/BodyPart.cs`：

- `Head`：头部
- `LeftArm`：左臂
- `RightArm`：右臂
- `Torso`：躯干
- `LeftLeg`：左腿
- `RightLeg`：右腿

## 与战斗系统的集成

### 攻击目标

- `BodyPart` 实现 `ICombatTarget` 接口，可在战斗中被选择为攻击目标
- 玩家选择攻击时，可以指定目标角色的身体部位
- 只有 `Available` 为 `true` 的部位才能被选择

### 格挡目标

- `GetBlockTargets()` 方法会返回角色的所有可用身体部位和装备
- 详细机制请参考 [物品系统文档](item-system.md#与战斗系统的集成) 和 [战斗系统文档](combat-system.md#反应系统)

### 伤害结算

- 每个身体部位拥有独立的生命值 `HitPoint`
- 当部位受到伤害时，生命值会减少
- 当生命值为 0 时，部位的 `Available` 属性返回 `false`
- 头部或躯干不可用时，角色被视为死亡

### 行动点系统

- 角色的 `actionPoint` 决定行动时机
- 每帧所有存活角色按 `speed.value * 0.1` 恢复行动点
- 当行动点达到上限时，角色可以执行行动
- 行动会消耗行动点（前摇和后摇）

### 反应系统

- 角色的 `reaction` 点数用于格挡或闪避，默认值为 1，每回合开始时重置为 1
- 详细机制请参考 [战斗系统文档](combat-system.md#反应系统) 中的反应系统章节

## 装备系统集成

### 装备挂载

- 身体部位通过 `Slots` 数组管理装备
- 默认情况下，双臂部位拥有 `ItemFlagCode.Arm` 类型的装备槽位
- 装备通过 `bodyPart.Slots[0].Item = item` 进行挂载
- 装备必须与槽位标志匹配才能挂载

### 装备作为格挡目标

- 装备可以作为格挡目标，详细机制请参考 [物品系统文档](item-system.md#与战斗系统的集成)

## 序列化与存档

### 角色序列化

角色的序列化格式：
1. 使用 `ReadScope()` 开始数据块
2. 名称（`string`）
3. 速度属性（`PropertyInt`）
4. 行动点属性（`PropertyDouble`）
5. 各身体部位数据（通过 `BodyPart.Deserialize()` 逐个读取）
6. 物品栏（`inventory.Serialize/Deserialize`，顺序为数量 → 各 `Item`）

### 身体部位序列化

身体部位的序列化格式：
1. 使用 `ReadScope()` 开始数据块
2. 生命值属性（`PropertyInt`）
3. 槽位数量（`int`）
4. 各槽位数据（通过 `ItemSlot.Deserialize()` 逐个读取，兼容性处理会忽略多余的槽位）

### 兼容性处理

- 反序列化时会检查槽位数量，安全处理版本差异
- 保证存档在不同版本间的兼容性

## 默认角色创建

### 创建玩家角色

在 `Game.CreateDefaultPlayers()` 中：
- 创建名为 "Hero" 的角色
- 如果在右臂有装备槽位，默认装备 `LongSword`

### 创建敌人角色

在 `Game.CreateDefaultEnemies()` 中：
- 创建名为 "Goblin" 的角色
- 如果在右臂有装备槽位，默认装备 `LongSword`

## 扩展指南

### 添加新身体部位

1. 在 `BodyPartCode` 枚举中添加新标识
2. 在 `BodyPartExtensions.GetName()` 中添加对应的中文名称
3. 在 `Character` 构造函数中添加新部位实例
4. 更新 `bodyParts` 列表

### 修改存活条件

- 当前存活条件为头部与躯干必须可用
- 如需修改，调整 `Character.IsAlive` 属性的实现逻辑

### 自定义身体部位装备槽

- 在创建 `BodyPart` 实例时，通过构造函数传入 `ItemSlot[]` 数组
- 每个槽位通过 `ItemFlagCode` 定义可接受的装备类型
- 可在任意部位添加任意类型的装备槽位

