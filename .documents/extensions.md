# 扩展方法

项目在 `Scripts/Extensions/` 目录下使用 `partial class Extensions` 组织常用扩展，覆盖事件调用、资源释放、二进制读写等高频场景。

## IDisposableExtensions

### TryDispose()

- 对实现 `IDisposable` 的对象执行安全释放，即使释放过程抛出异常也会捕获并记录
- 一般用于网络流、文件流等外部资源，避免清理失败导致主流程崩溃
- 异常通过 `Log.PrintException()` 打印，便于排查释放失败原因

## ActionExtensions

### TryInvoke<T>()

- 触发事件或委托时捕获订阅者抛出的异常
- 有效防止单个监听者的异常影响整个通知流程
- 所有异常统一走 `Log.PrintException()`，仍可在调试日志中查看完整堆栈

## BinaryReaderWriterExtensions

- 提供 `ReadScope()`、`WriteScope()`，以长度前缀的方式封装二进制块
- `ReaderBlock`、`WriterBlock` 使用 `IDisposable` 模式，自动在 `using` 块结束时对齐流位置
- 支持传入字符串或整数键值，便于快速校验数据结构版本是否匹配
- 建议所有可序列化结构都使用该模式，避免错读导致的数据错位

## GodotObjectExtensions

### Valid()

- 简化 `GodotObject.IsInstanceValid()` 的调用，用于判断节点是否仍然存在
- 常见于异步流程中访问节点前的防御性检查
- 配合 `this.Valid()` 可以在节点方法内快速判断自身是否已被释放

