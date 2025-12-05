# sync_commands

同步各 IDE 的命令文件夹，确保 `.workflows/`、`.claude/commands/`、`.github/prompts/` 三个目录保持一致。

## 步骤

1. 读取 `.workflows/` 目录下的所有 `.md` 文件列表（作为基准）

2. 检查 `.claude/commands/` 目录：
   - 对于每个工作流文件 `xxx.md`，检查是否存在对应的 `xxx.md`
   - 如果缺失，创建文件，格式为：
     ```
     简短的命令描述
     
     按照此[文档](/.workflows/xxx.md)执行
     ```

3. 检查 `.github/prompts/` 目录：
   - 对于每个工作流文件 `xxx.md`，检查是否存在对应的 `xxx.prompt.md`
   - 如果缺失，创建文件，格式为：
     ```
     # xxx
     
     按照此[文档](../../.workflows/xxx.md)执行
     ```

4. 报告同步结果：
   - 列出已存在的文件
   - 列出新创建的文件
   - 列出多余的文件（在命令目录中存在但工作流中不存在的）
