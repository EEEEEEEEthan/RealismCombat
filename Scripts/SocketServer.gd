extends Node

var tcp_server: TCPServer
var clients: Array[StreamPeerTCP] = []
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
			clients.append(client)
			print("SocketServer: 新客户端连接，当前连接数: ", clients.size())
	
	for i in range(clients.size() - 1, -1, -1):
		var client = clients[i]
		
		if client.get_status() != StreamPeerTCP.STATUS_CONNECTED:
			print("SocketServer: 客户端断开连接")
			clients.remove_at(i)
			continue
		
		var available = client.get_available_bytes()
		if available > 0:
			var data = client.get_utf8_string(available)
			_handle_message(client, data)

func _handle_message(client: StreamPeerTCP, message: String) -> void:
	message = message.strip_edges()
	print("SocketServer: 收到消息: ", message)
	
	if message == "hello":
		send_message(client, "world")
	else:
		send_message(client, "unknown command: " + message)

func send_message(client: StreamPeerTCP, message: String) -> void:
	if client.get_status() != StreamPeerTCP.STATUS_CONNECTED:
		print("SocketServer: 无法发送消息，客户端未连接")
		return
	
	var data = (message + "\n").to_utf8_buffer()
	client.put_data(data)
	print("SocketServer: 发送消息: ", message)

func _exit_tree() -> void:
	for client in clients:
		client.disconnect_from_host()
	clients.clear()
	
	if tcp_server != null:
		tcp_server.stop()
	print("SocketServer: 已关闭")

