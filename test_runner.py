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
import argparse
from datetime import datetime

TEST_DOC_CONTENT = """用中文沟通.
利用mcp工具运行游戏,并执行测试
1. 使用`system_launch_program`启动游戏客户端，等待命令返回主菜单选项。
2. 通过`game_select_option`选择游戏选项，推进流程
游戏文档见`.documents/index.md`
"""

def _sanitize_prompt(text: str) -> str:
	"""
	将prompt内容进行安全清洗，去除可能影响命令行的字符
	"""
	s = text.replace("\r", "")
	s = s.replace("\n", "  ")
	s = s.replace("\t", "")
	s = s.replace(" ", "")
	s = s.replace("'", "")
	s = s.replace("\"", "")
	s = s.replace("`", "")
	s = s.replace("$", "")
	s = s.replace("%", "")
	s = s.replace("&", "")
	return s

def read_output(pipe, log_file, debug=True):
    """
    从管道读取输出并实时显示和写入日志
    
    Args:
        pipe: 管道对象
        log_file: 日志文件对象
        debug: 是否打印到控制台
    """
    try:
        for line in iter(pipe.readline, ''):
            if line:
                line_str = line.rstrip()
                if line_str:
                    if debug:
                        print(line_str)
                    if log_file:
                        log_file.write(line)
                        log_file.flush()
    except Exception as e:
        if debug:
            print(f"读取输出时出错: {e}")
    finally:
        pipe.close()

def qwen(prompt_text, log_path, callback):
	"""
	非阻塞式启动qwen命令，实时记录stdout/stderr到log_path，进程结束后调用回调
	
	Args:
	    prompt_text: 原始prompt文本
	    log_path: 日志文件路径
	    callback: 回调函数，签名为callback(return_code)
	
	Returns:
	    subprocess.Popen 对象（调用方可自行决定是否等待）
	"""
	import threading
	import os
	
	# 确保日志目录存在
	log_dir = os.path.dirname(log_path) if log_path else ""
	if log_dir:
		os.makedirs(log_dir, exist_ok=True)
	
	# 生成命令
	sanitized = _sanitize_prompt(prompt_text)
	command_str = f"qwen -p -y \"{sanitized}\""
	
	# 打开日志文件
	log_file = open(log_path, 'w', encoding='utf-8') if log_path else None
	if log_file:
		log_file.write(f"命令: {command_str}\n")
		log_file.write("=" * 80 + "\n")
		log_file.flush()
	
	# 启动子进程（非阻塞）
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
	
	# 启动输出读取线程
	stdout_thread = threading.Thread(
		target=read_output,
		args=(process.stdout, log_file, False),
		daemon=True
	)
	stderr_thread = threading.Thread(
		target=read_output,
		args=(process.stderr, log_file, False),
		daemon=True
	)
	stdout_thread.start()
	stderr_thread.start()
	
	# 监视线程：等待进程结束，收尾并调用回调
	def _watch():
		try:
			return_code = process.wait()
		except Exception:
			return_code = -1
		finally:
			try:
				stdout_thread.join(timeout=1)
				stderr_thread.join(timeout=1)
			except Exception:
				pass
			if log_file:
				try:
					log_file.write("=" * 80 + "\n")
					log_file.write(f"进程返回码: {return_code}\n")
					log_file.flush()
					log_file.close()
				except Exception:
					pass
			try:
				if callable(callback):
					callback(return_code)
			except Exception:
				# 回调异常不向外抛出，避免打断调用方
				pass
	
	threading.Thread(target=_watch, daemon=True).start()
	return process

def main():
    parser = argparse.ArgumentParser(
        description='测试运行脚本 - 执行qwen命令进行测试',
        formatter_class=argparse.RawDescriptionHelpFormatter
    )
    parser.add_argument(
        'test',
        type=str,
        help='测试内容（必填，位置参数）'
    )
    args = parser.parse_args()
    test_content = args.test
    current_time = datetime.now()
    time_str = current_time.strftime('%Y_%m_%d_%H_%M')
    report_filename = f"report_{time_str}.md"
    log_filename = f"log_{time_str}.log"
    log_dir = ".testreports"
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)
    log_path = os.path.join(log_dir, log_filename)
	prompt = f"{TEST_DOC_CONTENT}\n测试内容:{test_content}.\n将测试报告输出到`/.testreports/{report_filename}`"
    
	try:
		def _on_finish(rc: int):
			print("-" * 80)
			print(f"进程返回码: {rc}")
			print(f"日志已保存到: {log_path}")
			print(f"测试报告已保存到: {os.path.join(log_dir, report_filename)}")
		process = qwen(prompt, log_path, _on_finish)
		# main保持原有阻塞行为，等待完成后返回码
		return_code = process.wait()
		return return_code
	except FileNotFoundError:
		error_msg = f"错误: 找不到命令 'qwen'，请确保qwen已安装并在PATH中"
		print(error_msg)
		return 1
	except Exception as e:
		error_msg = f"错误: 执行命令时发生异常: {e}"
		print(error_msg)
		return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)
