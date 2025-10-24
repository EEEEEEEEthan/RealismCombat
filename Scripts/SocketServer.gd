extends Node

var tcp_server: TCPServer
var port: int = 9999

func _ready() -> void:
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
			print("SocketServer: 接收短连接")
			_handle_short_connection(client)

func _handle_short_connection(client: StreamPeerTCP) -> void:
	var timeout = 100
	var elapsed = 0
	
	while elapsed < timeout:
		var available = client.get_available_bytes()
		if available > 0:
			var data = client.get_utf8_string(available)
			var message = data.strip_edges()
			print("SocketServer: 收到请求: ", message)
			
			var response = _process_request(message)
			
			var response_data = response.to_utf8_buffer()
			client.put_data(response_data)
			print("SocketServer: 发送响应: ", response)
			
			await get_tree().process_frame
			client.disconnect_from_host()
		return
	
	await get_tree().process_frame
	elapsed += 1
	
	print("SocketServer: 等待数据超时")
	client.disconnect_from_host()

func _process_request(request: String) -> String:
	if request == "hello":
		return "world"
	elif request == "system.shutdown":
		print("SocketServer: 收到关闭命令，准备退出游戏")
		get_tree().call_deferred("quit")
		return "游戏即将关闭"
	else:
		return "unknown command: " + request

func _exit_tree() -> void:
	if tcp_server != null:
		tcp_server.stop()
	print("SocketServer: 已关闭")
