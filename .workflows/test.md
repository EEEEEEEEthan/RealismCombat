# test $index

利用mcp工具运行游戏,并执行测试
1. 使用`system_launch_program`启动游戏客户端，等待命令返回主菜单选项。
2. 通过`game_select_option`选择游戏选项，推进流程
如果没有指定测试内容，默认需要测试本次对话的内容。
游戏文档见`.documents/index.md`
如果指定了$indexxx,进入游戏加载这个槽的存档.如果没有指定,默认加载0号存档.如果有特殊说明开始新游戏,则开始新游戏
