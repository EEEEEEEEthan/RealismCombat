# codeoptmize

1. 阅读需求, 明确优化范围, 确保不会改变既定行为
2. 分析现有代码结构, 找出冗余逻辑与重复实现
3. 利用提取方法、合并条件、拆分长函数等方式精简逻辑, 提升可读性
4. 参考[开发规范](../.documents/coding-standards.md)의架构约定, 确保依赖注入、可空处理与初始化原子性符合项目要求
5. 遵循项目风格, 为关键逻辑补充必要的文档注释, 避免行内注释
6. 如果有任何代码(.cs文件)变更,使用`cleanupcode.exe RealismCombat.sln --include="文件1;文件2;文件3" --profile="full" --settings=RealismCombat.sln.DotSettings` 格式化代码. 多个文件用分号分隔. 如果没有代码变更请跳过此步骤。
7. 完成后自查差异, 确认未引入新的警告或错误, 并记录潜在风险
