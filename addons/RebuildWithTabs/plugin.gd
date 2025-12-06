@tool
extends EditorPlugin

var rebuild_button: Button
var saved_scenes: Array[String] = []
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
	
	if not editor_interface:
		print("无法获取编辑器接口")
		return
	
	var open_scenes = editor_interface.get_open_scenes()
	
	for scene_path in open_scenes:
		saved_scenes.append(scene_path)
		print("保存场景标签页: ", scene_path)

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
	
	for scene_path in saved_scenes:
		if ResourceLoader.exists(scene_path):
			editor_interface.open_scene_from_path(scene_path)
			print("恢复场景标签页: ", scene_path)
		else:
			print("场景文件不存在: ", scene_path)

