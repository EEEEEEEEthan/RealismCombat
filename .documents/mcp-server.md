# MCP服务器设计

## 概述

游戏通过MCP(Model Context Protocol)服务器进行外部控制和自动化测试。

## 架构

### MCP客户端 (.McpServer/)
- 负责启动游戏进程
- 分配和管理端口
- 通过TCP连接与游戏通信
- 提供MCP工具接口供AI使用

### 游戏端服务器 (Scripts/McpServer/)
- GameServer：TCP服务器，监听指定端口
- 接收客户端命令并返回响应
- 在主线程中处理命令（通过并发队列）

### 程序入口 (Scripts/Nodes/)
- ProgramRootNode：游戏主入口节点
- 从命令行参数读取`--port=xxx`
- 初始化GameServer
- 客户端断开时自动退出

## 启动流程

1. MCP客户端分配空闲端口
2. 启动Godot进程并传递 `--port=xxx` 参数
3. 游戏启动TCP服务器监听该端口
4. MCP客户端连接到游戏
5. 双向通信建立完成

## 工具类

### Log
统一的日志输出接口，封装GD.Print系列方法。

### Settings
配置管理，从`.local.settings`文件读取配置（格式：`key = value`）。

