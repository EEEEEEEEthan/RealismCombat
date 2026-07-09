# C# 补充说明

## 优化检查清单

- [ ] 只读属性如果没有特殊逻辑（如计算、延迟初始化等），应该改为 `readonly` 字段以减少类成员
- [ ] 属性可以改为 `field` 关键字的要用 `field`（见下文「`field` 属性」）
- [ ] C# 的 `const` 只能用于基本类型；非基本类型用 `static readonly`
- [ ] 跨帧的状态应该封装为 `IEnumerator`，嵌套 `while` 表达子状态，用 `_process` 推进
- [ ] 完全信任 nullable 语法警告，不做任何多余的防御代码
- [ ] **优先使用现代 C# 特性简化代码**：
  - 如果可能，使用主构造函数 `(Type param)` 替代传统构造函数
  - 使用集合表达式 `= []` 替代 `new List<T>()`
  - 如果可能，使用 `foreach` 替代 `for` 循环（除非需要索引）
  - 使用 `??=` 和 `?.` 等空值合并操作符
- [ ] **避免过度复杂化**：
  - 不要添加不必要的中间变量或属性来跟踪状态
  - 如果属性本身已经缓存了值，不需要额外的缓存跟踪属性
  - 缓存失效逻辑应该尽可能简单直接
  - 移除所有未被使用的方法和属性
- [ ] **属性缓存（避免冗余 API/引擎调用）**（见下文）

## `field` 属性

- `[field: MaybeNull] NodeType NodeName => field ??= GetNode<NodeType>("%NodeName")`
- `NodeType? NodeName => field ??= GetNodeOrNull<NodeType>("NodeName")`
- 属性非空，`field` 可能为空需要增加 `[field: MaybeNull]` 标签
- `field` 语法在一些 IDE 里会被误报语法错误，但他是正确的语法

## 属性缓存（避免冗余 API/引擎调用）

- 属性名用 `XxxPosition`/`XxxRotation` 等语义名；backing field 用同名小写 `xxxPosition`，不要加 `_cached` 前缀
- setter：先 `if (cache == value) return`，再 `cache = 引擎对象.属性 = value` 合并赋值
- 缓存类型可用 `Vector3?`，getter 用 `cache ?? Target.Property` 兜底
