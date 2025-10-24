extends Node

class_name SocketServer

var tcp_server: TCPServer
var port: int = 9999
var _game: Game

func _init(game: Game) -> void:
	_game = game
	_parse_port_from_args()
	_start_server()

func _parse_port_from_args() -> void:
	var args = OS.get_cmdline_args()
	for arg in args:
		if arg.begins_with("--port="):
			var port_str = arg.substr(7)
			port = int(port_str)
			print("SocketServer: 从命令行参数解析端口号: ", port)
			return
	print("SocketServer: 使用默认端口: ", port)

func _start_server() -> void:
	tcp_server = TCPServer.new()
	var err = tcp_server.listen(port)
	if err != OK:
		print("SocketServer: 启动失败，错误码: ", err)
		return
	print("SocketServer: 成功启动，监听端口 ", port)

func _process(_delta: float) -> void:
	if tcp_server == null:
		return
	
	if tcp_server.is_connection_available():
		var client = tcp_server.take_connection()
		if client != null:
			print("SocketServer: 接收新连接")
			_handle_client_async(client)

func _handle_client_async(client: StreamPeerTCP) -> void:
	var start_time = Time.get_ticks_msec()
	var timeout_ms = 5000
	
	while (Time.get_ticks_msec() - start_time) < timeout_ms:
		if client.get_status() != StreamPeerTCP.STATUS_CONNECTED:
			print("SocketServer: 客户端断开连接")
			return
		
		var available = client.get_available_bytes()
		if available > 0:
			var data = client.get_utf8_string(available)
			var message = data.strip_edges()
			print("SocketServer: 收到请求: ", message)
			
			var response = await _process_request_async(message)
			
			if client.get_status() == StreamPeerTCP.STATUS_CONNECTED:
				var response_data = response.to_utf8_buffer()
				client.put_data(response_data)
				print("SocketServer: 发送响应: ", response)
			
			await _game.get_tree().process_frame
			client.disconnect_from_host()
			return
		
		await _game.get_tree().process_frame
	
	print("SocketServer: 等待数据超时")
	if client.get_status() == StreamPeerTCP.STATUS_CONNECTED:
		client.disconnect_from_host()

func _process_request_async(request: String) -> String:
	await _game.get_tree().process_frame
	if request == "system.shutdown":
		print("SocketServer: 收到关闭命令，准备退出游戏")
		_game.get_tree().call_deferred("quit")
		return "游戏即将关闭"
	else:
		return _game.exec_command(request)

func _exit_tree() -> void:
	if tcp_server != null:
		tcp_server.stop()
	print("SocketServer: 已关闭")
