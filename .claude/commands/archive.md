# archive

1. 使用 `git add` 添加所有修改的文件（包括用户可能已添加的其他文件）

2. 更新文档
   - 检查 `git diff --cached` 查看所有已暂存的变更
   - 如果有设计概念层面的变更，更新 `/.documents/` 目录下的相关文档
   - 只记录设计概念，不记录更新历史或实现细节
   - 如果没有值得记录的内容，跳过此步骤

3. 提交到 Git：
   - 根据 `git diff --cached` 的变更内容编写提交信息
   - 提交, `git commit -m "提交信息" --author="cursor and USER <MAIL>"`覆盖作者. USER和MAIL用原作者的设置
