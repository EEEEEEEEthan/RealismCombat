# removecommand

移除指定的指令

1. 确认要移除的指令名称（例如：`xxx`）

2. 删除以下三个文件：
   - `.claude/commands/xxx.md`
   - `.workflows/xxx.md`
   - `.github/prompts/xxx.prompt.md`

3. 如果文件不存在，提示用户并跳过

4. 删除完成后，确认三个文件都已成功移除


