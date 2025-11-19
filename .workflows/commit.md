# commit

1. 使用 `git add` 添加你修改的所有文件

2. diff：
   - `git diff --cached` 确认变更.注意可能有用户已经add的部分，这部分你需要一同编写提交日志

3. commit:
   - 提交, `git commit -m "提交信息" --author="YOURNAME and USER <MAIL>"`覆盖作者.YOURNAME是你的名字(例如claude,cursor,copilot,qwen,trae)USER和MAIL用原作者的设置
   - 如果乱码用英文重试

4. 其他脏文件保持原样
