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
利用mcp工具运行游戏,并执行测试。
1. 使用`system_launch_program`启动游戏客户端，等待命令返回主菜单选项。
2. 通过`game_select_option`选择游戏选项，推进流程
游戏文档见`.documents/index.md`
你可以参考`git diff`，结合用户输入的测试内容，推理出测试重点。注意边界问题以及是否影响到其他功能。
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
	command_str = f"qwen -p -y -d \"{sanitized}\""
	
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

def multi_test(prompt_text: str, num: int):
    """
    并发执行多次qwen测试（最多2个并发），每个实例独立日志与报告文件。
    全部结束后再启动一个新的qwen进程，汇总前面生成的.md，输出新的汇总.md。
    
    Args:
        prompt_text: 用户测试内容
        num: 运行实例数量
    
    Returns:
        最后汇总qwen进程的返回码
    """
    if num <= 0:
        return 0
    current_time = datetime.now()
    time_str = current_time.strftime('%Y_%m_%d_%H_%M')
    log_dir = ".testreports"
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)
    # 控制并发度为2
    semaphore = threading.Semaphore(2)
    completed = 0
    completed_lock = threading.Lock()
    all_done_event = threading.Event()
    return_codes = {}
    report_files = []
    processes = {}
    def _on_exit(rc, idx):
        nonlocal completed
        try:
            return_codes[idx] = rc
        finally:
            with completed_lock:
                completed += 1
                if completed >= num:
                    all_done_event.set()
        try:
            semaphore.release()
        except Exception:
            pass
    for i in range(1, num + 1):
        report_filename_i = f"report_{time_str}_{i}.md"
        log_filename_i = f"log_{time_str}_{i}.log"
        report_files.append(report_filename_i)
        log_path_i = os.path.join(log_dir, log_filename_i)
        prompt_i = f"{TEST_DOC_CONTENT}\n用户输入内容:{prompt_text}.\n将测试报告输出到`/{log_dir}/{report_filename_i}`"
        def _launch(idx=i, lp=log_path_i, ptxt=prompt_i):
            semaphore.acquire()
            processes[idx] = qwen(ptxt, lp, lambda rc, j=idx: _on_exit(rc, j))
        threading.Thread(target=_launch, daemon=True).start()
    all_done_event.wait()
    summary_report = f"report_{time_str}_summary.md"
    summary_log = f"log_{time_str}_summary.log"
    summary_log_path = os.path.join(log_dir, summary_log)
    files_list_str = "\n".join([f"- `/{log_dir}/{name}`" for name in report_files])
    summary_prompt = (
        f"请阅读并总结以下报告文件，合并结论、问题与建议，去重并标注来源：\n{files_list_str}\n"
        f"将测试报告输出到`/{log_dir}/{summary_report}`"
    )
    try:
        final_proc = qwen(summary_prompt, summary_log_path, None)
        final_code = final_proc.wait()
        return final_code
    except FileNotFoundError:
        print("错误: 找不到命令 'qwen'，请确保qwen已安装并在PATH中")
        return 1
    except Exception as e:
        print(f"错误: 执行汇总命令时发生异常: {e}")
        return 1

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
    parser.add_argument(
        '--repeat',
        type=int,
        default=2,
        help='重复运行次数（默认2）。<=0报错；1为单次直接运行；>1为并发multi_test'
    )
    args = parser.parse_args()
    test_content = args.test
    repeat = args.repeat
    current_time = datetime.now()
    time_str = current_time.strftime('%Y_%m_%d_%H_%M')
    log_dir = ".testreports"
    if not os.path.exists(log_dir):
        os.makedirs(log_dir)
    if repeat <= 0:
        print("错误: --repeat 必须为正整数")
        return 1
    if repeat == 1:
        report_filename = f"report_{time_str}_summary.md"
        log_filename = f"log_{time_str}.log"
        log_path = os.path.join(log_dir, log_filename)
        prompt = f"{TEST_DOC_CONTENT}\n用户输入内容:{test_content}.\n将测试报告输出到`/{log_dir}/{report_filename}`"
        try:
            process = qwen(prompt, log_path, None)
            return_code = process.wait()
            print("-" * 80)
            print(f"进程返回码: {return_code}")
            print(f"日志已保存到: {log_path}")
            try:
                report_path = os.path.join(log_dir, report_filename)
                with open(report_path, 'r', encoding='utf-8') as f:
                    report_content = f.read()
                print(report_content)
            except Exception as e:
                print(f"读取测试报告失败: {e}")
            return return_code
        except FileNotFoundError:
            error_msg = f"错误: 找不到命令 'qwen'，请确保qwen已安装并在PATH中"
            print(error_msg)
            return 1
        except Exception as e:
            error_msg = f"错误: 执行命令时发生异常: {e}"
            print(error_msg)
            return 1
    else:
        rc = multi_test(test_content, repeat)
        try:
            summary_log = f"log_{time_str}_summary.log"
            summary_log_path = os.path.join(log_dir, summary_log)
        except Exception:
            pass
        return rc

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)
