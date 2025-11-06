# 扩展方法

项目提供了一些实用的扩展方法，定义在 `Scripts/Extensions/` 目录下，使用 `partial class` 模式组织：

## IDisposableExtensions

### TryDispose()

安全地释放资源，捕获并记录异常。

- 用于需要释放资源但不希望异常中断程序流程的场景
- 异常会通过 `Log.PrintException()` 记录

## ActionExtensions

### TryInvoke<T>()

安全地调用事件或委托，捕获并记录异常。

- 用于需要触发事件但不希望订阅者的异常中断程序流程的场景
- 异常会通过 `Log.PrintException()` 记录

