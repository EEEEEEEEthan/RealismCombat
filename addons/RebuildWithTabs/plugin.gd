@tool
extends EditorPlugin

var rebuild_button: Button
var saved_scenes: Array[String] = []
var saved_current_scene: String = ""
var editor_interface: EditorInterface

func _enter_tree():
	editor_interface = get_editor_interface()
	
	rebuild_button = Button.new()
	rebuild_button.text = "Rebuild"
	rebuild_button.pressed.connect(_on_rebuild_pressed)
	add_control_to_container(CONTAINER_TOOLBAR, rebuild_button)

func _exit_tree():
	if rebuild_button:
		remove_control_from_container(CONTAINER_TOOLBAR, rebuild_button)
		rebuild_button.queue_free()

func _on_rebuild_pressed():
	save_current_tabs()
	await close_all_tabs()
	rebuild_project()

func save_current_tabs():
	saved_scenes.clear()
	saved_current_scene = ""
	
	if not editor_interface:
		print("无法获取编辑器接口")
		return
	
	var open_scenes = editor_interface.get_open_scenes()
	
	if open_scenes.is_empty():
		return
	
	var editor_main_screen = editor_interface.get_editor_main_screen()
	if editor_main_screen:
		var base_control = editor_interface.get_base_control()
		var scene_tabs = base_control.find_child("SceneTabs", true, false)
		if scene_tabs:
			var current_tab_index = scene_tabs.current_tab
			if current_tab_index >= 0 and current_tab_index < open_scenes.size():
				saved_current_scene = open_scenes[current_tab_index]
	
	if saved_current_scene == "" and not open_scenes.is_empty():
		saved_current_scene = open_scenes[0]
	
	for scene_path in open_scenes:
		saved_scenes.append(scene_path)
		print("保存场景标签页: ", scene_path)
	
	print("当前激活的场景: ", saved_current_scene)

func close_all_tabs():
	if not editor_interface:
		print("无法获取编辑器接口")
		return
	
	var editor_main_screen = editor_interface.get_editor_main_screen()
	if not editor_main_screen:
		print("无法获取编辑器主屏幕")
		var open_scenes = editor_interface.get_open_scenes()
		while not open_scenes.is_empty():
			editor_interface.close_scene()
			await Engine.get_main_loop().process_frame
			open_scenes = editor_interface.get_open_scenes()
		return
	
	var base_control = editor_interface.get_base_control()
	var scene_tabs = base_control.find_child("SceneTabs", true, false)
	
	if not scene_tabs:
		print("无法找到场景标签页节点，使用备用方法")
		var open_scenes = editor_interface.get_open_scenes()
		while not open_scenes.is_empty():
			editor_interface.close_scene()
			await Engine.get_main_loop().process_frame
			open_scenes = editor_interface.get_open_scenes()
		return
	
	var tab_count = scene_tabs.get_tab_count()
	print("当前场景标签页数量: ", tab_count)
	
	for i in range(tab_count - 1, -1, -1):
		scene_tabs.current_tab = i
		await Engine.get_main_loop().process_frame
		editor_interface.close_scene()
		await Engine.get_main_loop().process_frame
		print("关闭场景标签页: ", i)
	
	print("已关闭所有场景标签页")

func rebuild_project():
	print("开始重编译项目...")
	
	if not editor_interface:
		print("无法获取编辑器接口")
		return
	
	var project_path = ProjectSettings.globalize_path("res://")
	var csproj_path = project_path + "/RealismCombat.csproj"
	
	print("项目路径: ", project_path)
	print("C#项目文件: ", csproj_path)
	
	var output = []
	var exit_code = OS.execute("dotnet", ["build", csproj_path, "--nologo"], output, true, false)
	
	if exit_code == 0:
		print("C#项目编译成功")
		editor_interface.get_resource_filesystem().scan()
	else:
		print("C#项目编译失败，退出码: ", exit_code)
		for line in output:
			print(line)
	
	await Engine.get_main_loop().process_frame
	call_deferred("restore_tabs_after_delay")

func restore_tabs_after_delay():
	await Engine.get_main_loop().process_frame
	await Engine.get_main_loop().process_frame
	await Engine.get_main_loop().process_frame
	
	restore_tabs()

func restore_tabs():
	if saved_scenes.is_empty():
		print("没有需要恢复的标签页")
		return
	
	if not editor_interface:
		print("无法获取编辑器接口")
		return
	
	print("开始恢复标签页...")
	
	var scenes_to_open = []
	var current_scene_to_open = ""
	
	for scene_path in saved_scenes:
		if ResourceLoader.exists(scene_path):
			if scene_path == saved_current_scene:
				current_scene_to_open = scene_path
			else:
				scenes_to_open.append(scene_path)
		else:
			print("场景文件不存在: ", scene_path)
	
	for scene_path in scenes_to_open:
		editor_interface.open_scene_from_path(scene_path)
		print("恢复场景标签页: ", scene_path)
		await Engine.get_main_loop().process_frame
	
	if current_scene_to_open != "":
		editor_interface.open_scene_from_path(current_scene_to_open)
		print("恢复当前激活的场景: ", current_scene_to_open)
		await Engine.get_main_loop().process_frame
		
		var base_control = editor_interface.get_base_control()
		var scene_tabs = base_control.find_child("SceneTabs", true, false)
		if scene_tabs:
			var open_scenes = editor_interface.get_open_scenes()
			for i in range(open_scenes.size()):
				if open_scenes[i] == current_scene_to_open:
					scene_tabs.current_tab = i
					print("设置当前标签页为: ", i)
					break

