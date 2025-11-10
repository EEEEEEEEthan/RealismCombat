# commit

1. `git status` 查看状态
  如果有任何代码(.cs文件)变更,使用`cleanupcode.exe RealismCombat.sln --include="文件1;文件2;文件3" --profile="full" --settings=RealismCombat.sln.DotSettings` 格式化代码. 多个文件用分号分隔. 如果没有代码变更请跳过此步骤。
  已在环境变量中配置 `cleanupcode.exe`, 可直接调用。

2. 使用 `git add` 添加你修改的所有文件

3. 更新文档
   - 检查 `git diff --cached` 查看所有已暂存的变更.这个变更也可能包括除了你add以外,用户add的变更.需要一并检查.
   - 对每个已 add 的变更，判断是否涉及设计/规则/流程；如是，需在文档体系（如 `.documents`、`.doc`）中补充相应记录
   - 更新完文档之后,把文档变更添加到暂存区

4. 提交到 Git：
   - 根据 `git diff --cached` 的变更内容编写中文提交信息
   - 提交, `git commit -m "提交信息" --author="YOURNAME and USER <MAIL>"`覆盖作者.YOURNAME是你的名字(例如claude,cursor,copilot,qwen,trae)USER和MAIL用原作者的设置
   - 如果乱码用英文重试
