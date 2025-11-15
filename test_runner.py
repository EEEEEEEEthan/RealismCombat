#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
测试运行脚本
执行qwen命令进行测试
"""

import sys
import subprocess
import os
import platform
import threading
from datetime import datetime

# 测试文档内容（从.workflows/test.md复制）
TEST_DOC_CONTENT = """用中文沟通.
利用mcp工具运行游戏,并执行测试
1. 使用`system_launch_program`启动游戏客户端，等待命令返回主菜单选项。
2. 通过`game_select_option`选择游戏选项，推进流程
游戏文档见`.documents/index.md`
"""

def read_output(pipe, log_file):
    """
    从管道读取输出并实时显示和写入日志
    """
    try:
        for line in iter(pipe.readline, ''):
            if line:
                line_str = line.rstrip()
                if line_str:
                    print(line_str)
                    if log_file:
                        log_file.write(line)
                        log_file.flush()
    except Exception as e:
        print(f"读取输出时出错: {e}")
    finally:
        pipe.close()

def main():
    test_content = sys.argv[1] if len(sys.argv) > 1 else "常规测试"
    # 生成带时间戳的报告文件名和日志文件名
    current_time = datetime.now()
    time_str = current_time.strftime('%Y_%m_%d_%H_%M')
    report_filename = f"report_{time_str}.md"
    log_filename = f"log_{time_str}.log"
    
    # 确保.testreports目录存在
    log_dir = ".testreports"
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)
    
    log_path = os.path.join(log_dir, log_filename)
    # 构建prompt
    prompt = f"{TEST_DOC_CONTENT}  测试内容:{test_content}."
    prompt = prompt.replace("\r", "")
    prompt = prompt.replace("\n", "  ")
    prompt = prompt.replace("\t", "")
    prompt = prompt.replace(" ", "")
    prompt = prompt.replace("'", "")
    prompt = prompt.replace("\"", "")
    prompt = prompt.replace("`", "")
    prompt = prompt.replace("$", "")
    prompt = prompt.replace("%", "")
    prompt = prompt.replace("&", "")
    command_str = f"qwen -p -y \"{prompt}\""
    print(command_str)
    print("-" * 80)
    
    try:
        # 打开日志文件
        log_file = open(log_path, 'w', encoding='utf-8')
        log_file.write(f"命令: {command_str}\n")
        log_file.write("=" * 80 + "\n")
        log_file.flush()
        
        # 使用Popen启动进程，实时读取输出
        process = subprocess.Popen(
            command_str,
            shell=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            encoding='utf-8',
            errors='replace',
            bufsize=1
        )
        
        # 创建线程来实时读取stdout和stderr
        stdout_thread = threading.Thread(
            target=read_output,
            args=(process.stdout, log_file)
        )
        stderr_thread = threading.Thread(
            target=read_output,
            args=(process.stderr, log_file)
        )
        
        stdout_thread.daemon = True
        stderr_thread.daemon = True
        
        stdout_thread.start()
        stderr_thread.start()
        
        # 等待进程结束
        return_code = process.wait()
        
        # 等待读取线程完成
        stdout_thread.join(timeout=1)
        stderr_thread.join(timeout=1)
        
        # 写入结束信息
        log_file.write("=" * 80 + "\n")
        log_file.write(f"进程返回码: {return_code}\n")
        log_file.close()
        
        print("-" * 80)
        print(f"进程返回码: {return_code}")
        print(f"日志已保存到: {log_path}")
        return return_code
    except FileNotFoundError:
        print(f"错误: 找不到命令 'qwen'，请确保qwen已安装并在PATH中")
        if 'log_file' in locals():
            log_file.close()
        return 1
    except Exception as e:
        print(f"错误: 执行命令时发生异常: {e}")
        if 'log_file' in locals():
            log_file.close()
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)
