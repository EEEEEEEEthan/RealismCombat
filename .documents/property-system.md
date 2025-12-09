# 属性系统

## 概述

属性系统定义了游戏中通用的属性类，用于表示具有当前值和最大值的数值属性，如生命值、行动点、速度、耐久等。

## 核心类

### PropertyInt 类

`PropertyInt` 表示整数类型的属性，位于 `Scripts/Properties.cs`：

#### 属性

- `int value`：当前值
- `int maxValue`：最大值

#### 构造方法

- `PropertyInt(int value, int maxValue)`：使用指定的当前值和最大值创建属性
- `PropertyInt(BinaryReader reader)`：从二进制读取器反序列化属性

#### 序列化方法

- `Deserialize(BinaryReader reader)`：从二进制读取器反序列化属性
- `Serialize(BinaryWriter writer)`：将属性序列化到二进制写入器

#### 使用场景

- 生命值（`HitPoint`）：角色身体部位和装备的生命值/耐久
- 速度（`speed`）：角色的速度属性，决定每帧行动点回复量

### PropertyDouble 类

`PropertyDouble` 表示双精度浮点数类型的属性，位于 `Scripts/Properties.cs`：

#### 属性

- `double value`：当前值
- `double maxValue`：最大值

#### 构造方法

- `PropertyDouble(double value, double maxValue)`：使用指定的当前值和最大值创建属性
- `PropertyDouble(BinaryReader reader)`：从二进制读取器反序列化属性

#### 序列化方法

- `Serialize(BinaryWriter writer)`：将属性序列化到二进制写入器

注意：`PropertyDouble` 在序列化时使用 `float`（单精度）类型，序列化时调用 `writer.Write(value)` 会将 `double` 转换为 `float`，读取时也会以 `float` 读取。

#### 使用场景

- 行动点（`actionPoint`）：角色的行动点属性，记录当前与最大行动点

## 序列化格式

### PropertyInt 序列化

1. 使用 `ReadScope()` / `WriteScope()` 开始和结束数据块
2. 写入/读取 `value`（`int`）
3. 写入/读取 `maxValue`（`int`）

### PropertyDouble 序列化

1. 使用 `ReadScope()` / `WriteScope()` 开始和结束数据块
2. 写入/读取 `value`（`float`，从 `double` 转换）
3. 写入/读取 `maxValue`（`float`，从 `double` 转换）

## 使用示例

### 创建属性

```csharp
// 创建生命值属性：当前值 8，最大值 10
var hitPoint = new PropertyInt(8, 10);

// 创建行动点属性：当前值 5.5，最大值 10.0
var actionPoint = new PropertyDouble(5.5, 10.0);
```

### 修改属性值

```csharp
// 修改当前值
hitPoint.value = 10;
actionPoint.value = 7.5;

// 限制在当前值不超过最大值
hitPoint.value = Math.Min(hitPoint.value, hitPoint.maxValue);
```

### 序列化与反序列化

```csharp
// 序列化
using var stream = new FileStream("save.dat", FileMode.Create);
using var writer = new BinaryWriter(stream);
hitPoint.Serialize(writer);

// 反序列化
using var stream = new FileStream("save.dat", FileMode.Open);
using var reader = new BinaryReader(stream);
var hitPoint = new PropertyInt(reader);
```

## 在战斗系统中的应用

### 身体部位生命值

- `BodyPart.HitPoint` 使用 `PropertyInt` 存储生命值
- 不同身体部位的最大值不同：
  - 头部：最大值为 5
  - 左臂、右臂：最大值为 8
  - 左腿、右腿：最大值为 8
  - 躯干：最大值为 10
- 当 `HitPoint.value > 0` 时，部位可用（`Available` 返回 `true`）

### 装备耐久

- `Item.HitPoint` 使用 `PropertyInt` 存储耐久
- 默认最大值为 10
- 当 `HitPoint.value > 0` 时，装备可用（`Available` 返回 `true`）

### 角色速度

- `Character.speed` 使用 `PropertyInt` 存储速度
- 默认最大值为 5
- 运行时通过属性 `Speed` 输出最终速度：基础速度 5 乘以负重曲线（裸装总重量 75 时为 5，重量越大递减并无限趋近于 `5/3`），行动点回复量为 `Speed * 0.1`

### 角色行动点

- `Character.actionPoint` 使用 `PropertyDouble` 存储行动点
- 默认最大值为 10.0
- 当行动点达到上限时，角色可以执行行动
- 行动会消耗行动点（前摇和后摇）

## 设计考虑

### 为什么使用两个类型

- `PropertyInt` 用于整数值属性（如生命值、速度），精确无误差
- `PropertyDouble` 用于浮点值属性（如行动点），支持小数精度

### 序列化精度

- `PropertyDouble` 在序列化时使用 `float` 类型，会丢失精度
- 这是为了节省存储空间，对于游戏属性来说单精度已足够

### 作用域保护

- 使用 `ReadScope()`、`WriteScope()` 保证数据块完整性
- 如果数据格式不匹配，可以通过作用域检测并报告错误

## 扩展指南

### 添加新属性类型

1. 创建新的属性类（如 `PropertyFloat`、`PropertyLong`）
2. 实现 `value` 和 `maxValue` 属性
3. 实现序列化方法，使用 `ReadScope()`、`WriteScope()` 保证完整性
4. 在需要的地方使用新属性类型

### 添加属性验证

- 可以在属性的 setter 中添加验证逻辑
- 例如限制值在合理范围内，或在值变化时触发事件

