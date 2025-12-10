# 物品系统

## 概述

物品系统管理游戏中的装备实体，包括物品的定义、装备槽的管理、物品与身体部位的关联，以及在战斗中的应用。当前物品体系已配置化，物品由工厂方法结合内置配置生成。

## 核心类

### Item 类

`Item` 是具体的装备实体，位于 `Scripts/Items/Item.cs`：

- 实现 `ICombatTarget` 与 `IItemContainer`，可被选作战斗目标并支持嵌套装备
- 字段：`ItemIdCode id`、`ItemFlagCode flag`、`PropertyInt HitPoint`、`ItemSlot[] Slots`、`string Name`、`double Length`、`double Weight`、`string Icon`、`string IconTag`
- 通过 `Item.Create(ItemIdCode id)` 工厂方法创建，底层使用 `ItemConfig` 定义名称、标志、槽位、长度、重量、耐久
- 槽位由配置的 `SlotFlags` 生成，用于继续承载下一层装备
- `Icon` 存放资源路径；`IconTag` 为富文本标签，菜单展示时与装备名拼接，例如 `#皮带[img=8x8]res://...[/img]`

#### 序列化

- `Item.Load(BinaryReader reader)`：根据存档 ID 创建对应配置的物品，再反序列化槽位
- `Serialize(BinaryWriter writer)`：写入 ID、槽位数量与槽位数据（无子类特定分支）

### ItemSlot 类

`ItemSlot` 管理单个装备槽位，位于 `Scripts/Items/ItemSlot.cs`：

- 构造函数接受 `ItemFlagCode flag`，定义可接受的装备类型，并通过 `Flag` 属性对外暴露
- 赋值 `Item` 时会校验标志，不匹配将抛出 `ArgumentException`
- 支持二进制序列化，使用 `ReadScope()`、`WriteScope()` 保证数据块完整性
- 构造时必须传入所属容器（`IItemContainer`），槽位会持有只读 `container`/`Container` 引用；`Item` 与 `BodyPart` 在创建或反序列化时负责传入自身，额外跳过的冗余槽也会绑定当前容器

### ItemFlagCode 枚举

`ItemFlagCode` 位于 `Scripts/Items/ItemFlagCode.cs`，当前包含：

- `Arm`：武器
- `TorsoArmor`：内层上衣
- `TorsoArmorMiddle`：中层上衣
- `TorsoArmorOuter`：外层上衣
- `HandArmor`：护手
- `LegArmor`：内层腿甲
- `LegArmorMiddle`：中层腿甲
- `LegArmorOuter`：外层腿甲
- `HeadArmor`：内层头盔
- `HeadArmorMiddle`：中层头盔
- `HeadArmorOuter`：外层头盔
- `FootArmor`：鞋子
- `Belt`：皮带

护甲聚合标志 `Armor` 覆盖头盔三层、上衣三层、裤子三层、护手与鞋子。

#### 显示名称

`ItemFlagCodeExtensions.DisplayName()` 将标志组合转换为中文名称（以`、`分隔），用于菜单提示等 UI 场景。

### ItemIdCode 枚举与内置配置

`ItemIdCode` 位于 `Scripts/Items/Item.cs`，内置物品及槽位结构：

- `LongSword`：武器，标志 `Arm`，无槽位
- `CottonLiner`：武装衣，标志 `TorsoArmor`，槽位 `TorsoArmorMiddle`
- `ChainMail`：链甲，标志 `TorsoArmorMiddle`，槽位 `TorsoArmorOuter`
- `PlateArmor`：板甲，标志 `TorsoArmorOuter`，无槽位
- `Belt`：皮带，标志 `Belt`，4 个 `Arm` 槽位
- `CottonPants`：武装腿甲，标志 `LegArmor`，槽位 `LegArmorMiddle`，覆盖髋至膝，缓冲与保暖为主
- `ChainChausses`：链甲腿甲，标志 `LegArmorMiddle`，槽位 `LegArmorOuter`，覆盖髋至膝，链环防护为主
- `PlateChausses`：板甲腿甲，标志 `LegArmorOuter`，无槽位，小块钢板铆接覆盖髋至膝

### IItemContainer 接口

`IItemContainer` 定义 `ItemSlot[] Slots`，由 `Item` 与 `BodyPart` 实现，用于统一处理嵌套与部位装备。
接口不再提供附加装备名称的帮助方法，需要展示时在调用侧直接遍历槽位拼接 `IconTag`。

### Inventory 类

`Inventory` 位于 `Scripts/Items/Inventory.cs`，仅持有 `List<Item> Items`，并提供序列化与反序列化。

## 与战斗系统的集成

### 装备挂载

- `BodyPart` 通过 `Slots` 管理装备槽位；默认双臂有 `Arm` 槽，躯干有 `TorsoArmor` 与 `Belt` 槽，裆部有 `LegArmor` 槽
- 双腿各有一个 `FootArmor` 槽用于鞋子
- 头部新增 `HeadArmor` 槽，用于承载头盔内层；中层、外层需由内层头盔继续提供子槽（同上衣/裤子层级思路）
- 装备通过 `slot.Item = Item.Create(...)` 挂载，标志不匹配会抛出异常
- 装备菜单仅展示存在装备槽的身体部位；无槽部位不会出现在选择列表中。角色若没有可装备的部位，会提示“没有可装备的部位”并返回上级菜单
- 选择物品栏中的装备成功换装后不再弹出成功提示，直接返回
- 装备菜单与物品栏列表的描述统一通过 `Game.FormatItemDescription` 生成：首行显示装备 `flag` 的中文名称，次行显示原始物品描述，便于区分类型与详情

### 战斗中的暂存与归还

- `Combat` 在开战时记录每件已装备物品对应的槽位与所属角色，含多层嵌套
- 战斗结束时优先将物品放回原槽；若原槽已被其他物品占用，则先把占用物移入物品栏再归还；若原容器已不在角色身上则跳过归还

### 战斗目标

- `Item` 实现 `ICombatTarget`，可出现在格挡与伤害结算目标列表中
- `GetBlockTargets()` 会返回角色可用的身体部位与当前装备

### 伤害结算

- 装备拥有独立耐久 `HitPoint`，降为 0 时 `Available` 为 `false`

## 序列化与存档

### 序列化格式

物品序列化使用 `BinaryReaderWriterExtensions` 的作用域机制：
- 顺序：ID（`ulong`）→ 槽位数量（`int`）→ 各槽位数据

### 兼容性处理

- 反序列化会根据当前物品的槽位数量截断或跳过多余数据，保证版本兼容

## 配置扩展

- 新增物品：在 `ItemIdCode` 与 `Item.Configs` 中添加条目，配置名称、标志、槽位、长度、重量、耐久
- 新增装备类型：在 `ItemFlagCode` 增加标志，并在需要承载该类型的部位或物品上添加对应槽位
- 嵌套示例：躯干(`TorsoArmor`槽) → 武装衣(`TorsoArmorMiddle`) → 链甲(`TorsoArmorOuter`) → 板甲 → 皮带(4×`Arm`)；裆部(`LegArmor`槽) → 武装腿甲(`LegArmorMiddle`) → 链甲腿甲(`LegArmorOuter`) → 板甲腿甲
