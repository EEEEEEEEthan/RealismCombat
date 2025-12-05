# addcommand

添加新的指令到工作流系统

1. 在 `.workflows/` 目录下创建新的工作流文档（如 `xxx.md`）
   - 文档应包含详细的步骤说明
   - 参考现有工作流文档的格式和风格

2. 在 `.claude/commands/` 目录下创建对应的命令文件（如 `xxx.md`）
   - 第一行：简短的命令描述
   - 空行
   - 第二段：按照此[文档](/.workflows/xxx.md)执行

3. 在 `.github/prompts/` 目录下创建对应的 prompt 文件（如 `xxx.prompt.md`）
   - 标题：`# xxx`
   - 内容：按照此[文档](../../.workflows/xxx.md)执行

4. 确保三个文件中的引用路径正确：
   - `.claude/commands/` 中使用：`/.workflows/xxx.md`
   - `.github/prompts/` 中使用：`../../.workflows/xxx.md`

