# 游戏颜色系统

## 概述

游戏颜色系统定义了游戏中会复用的颜色常量，提供统一的颜色梯度，用于 UI 样式、视觉效果等场景。

## 核心类

### GameColors 类

`GameColors` 是静态工具类，位于 `Scripts/GameColors.cs`，提供游戏中使用的颜色常量：

#### 透明色

- `transparent`：透明色（`Colors.Transparent`）

#### 颜色梯度

所有颜色梯度都是从浅到深排列的只读列表（`IReadOnlyList<Color>`）：

##### grayGradient

灰色梯度，包含 6 种颜色：
- `#ffffff`：白色
- `#ebebeb`：浅灰
- `#b2b2b2`：中灰
- `#a2a2a2`：深灰
- `#797979`：更深灰
- `#000000`：黑色

##### skyBlueGradient

天蓝色梯度，包含 4 种颜色：
- `#a2baff`：浅天蓝
- `#5182ff`：中天蓝
- `#4141ff`：深天蓝
- `#2800ba`：深蓝

##### sunFlareOrangeGradient

太阳光橙色梯度，包含 4 种颜色：
- `#ffcbba`：浅橙
- `#ff7930`：中橙
- `#e35100`：深橙
- `#e23000`：深红橙

##### pinkGradient

粉色梯度，包含 4 种颜色：
- `#ffbaeb`：浅粉
- `#ff61b2`：中粉
- `#db4161`：深粉
- `#b21030`：深红

## 使用场景

### UI 样式

颜色梯度可用于：
- 按钮样式
- 背景渐变
- 文本颜色
- 边框颜色

### 视觉效果

- `pinkGradient` 的最后一个颜色（`#b21030`）用于 `PropertyNode.FlashRed()` 方法，在角色受击时闪烁红色
- 颜色梯度可用于创建平滑的视觉效果

### 主题定制

- 通过修改 `GameColors` 中的颜色定义，可以统一调整游戏的视觉风格
- 所有使用这些颜色的地方会自动应用新的颜色方案

## 使用示例

### 访问颜色

```csharp
// 获取灰色梯度的第一个颜色（白色）
var white = GameColors.grayGradient[0];

// 获取粉色梯度的最后一个颜色（深红）
var darkRed = GameColors.pinkGradient[^1];

// 获取透明色
var transparent = GameColors.transparent;
```

### 在 UI 中使用

```csharp
// 设置按钮颜色
button.Modulate = GameColors.skyBlueGradient[1];

// 闪烁红色效果
node.Modulate = GameColors.pinkGradient[^1];
```

### 创建渐变效果

```csharp
// 使用颜色梯度创建渐变
for (int i = 0; i < GameColors.grayGradient.Count; i++)
{
    var color = GameColors.grayGradient[i];
    // 应用到 UI 元素
}
```

## 设计原则

### 统一颜色管理

- 所有游戏颜色集中在 `GameColors` 类中定义
- 避免在代码中硬编码颜色值
- 便于统一维护和调整视觉风格

### 颜色梯度

- 颜色按从浅到深的顺序排列
- 梯度包含多个颜色值，可用于创建平滑过渡效果
- 梯度列表是只读的，确保颜色定义不被意外修改

## 扩展指南

### 添加新颜色

1. 在 `GameColors` 类中添加新的颜色常量或梯度
2. 使用 `new Color("#hexcode")` 创建颜色实例
3. 如果是梯度，使用数组字面量语法创建 `IReadOnlyList<Color>`

### 修改现有颜色

- 直接修改 `GameColors` 类中的颜色定义
- 所有使用该颜色的地方会自动应用新颜色
- 注意：修改会影响所有使用该颜色的 UI 和效果

### 创建颜色工具方法

- 可以在 `GameColors` 类中添加静态方法，如 `LerpColor()`、`Brighten()` 等
- 用于动态生成颜色变化

