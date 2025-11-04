# archive

1. `cleanupcode.exe RealismCombat.sln --profile="full" --settings=RealismCombat.sln.DotSettings` 格式化代码

2. 使用 `git add` 添加所有修改的文件（包括用户可能已添加的其他文件）

3. 更新文档
   - 检查 `git diff --cached` 查看所有已暂存的变更
   - 如果有设计概念层面的变更，更新 `根目录/.documents/` 目录下的相关文档
   - 只记录设计概念，不记录更新历史或实现细节
   - 如果没有值得记录的内容，跳过此步骤
   - 更新完文档之后,把文档变更添加到暂存区

4. 提交到 Git：
   - 根据 `git diff --cached` 的变更内容编写中文提交信息
   - 提交, `git commit -m "提交信息" --author="YOURNAME and USER <MAIL>"`覆盖作者.YOURNAME是你的名字(例如claude,cursor,copilot,qwen,trae)USER和MAIL用原作者的设置
   - 如果乱码用英文重试
