using System;
using System.IO;
using System.Threading.Tasks;
using Godot;
using FileAccess = System.IO.FileAccess;
/// <summary>
///     程序根节点，负责初始化游戏生命周期
/// </summary>
public partial class ProgramRoot : Node
{
	const int SaveSlotCount = 7;
	static string GetSaveFilePath(int slotIndex)
	{
		EnsureSaveDirectoryExists();
		return ProjectSettings.GlobalizePath($"user://save_slot_{slotIndex + 1}.sav");
	}
	static void EnsureSaveDirectoryExists()
	{
		var directoryPath = ProjectSettings.GlobalizePath("user://");
		if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
	}
	/// <summary>
	///     创建槽位菜单项。
	/// </summary>
	static MenuOption CreateSaveSlotOption(int slotIndex)
	{
		var filePath = GetSaveFilePath(slotIndex);
		if (File.Exists(filePath))
			try
			{
				using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryReader(stream);
				var snapshot = new Snapshot(reader);
				return new()
				{
					title = $"#{slotIndex + 1} {snapshot.Title}",
					description = $"{snapshot.Desc}",
				};
			}
			catch (Exception e)
			{
				Log.PrintException(e);
			}
		return new()
		{
			title = $"#{slotIndex + 1} 空",
			description = "暂无存档",
		};
	}
	/// <summary>
	///     选择存档槽位。
	/// </summary>
	static async Task<int?> SelectSaveSlot(bool requireExisting)
	{
		while (true)
		{
			var options = new MenuOption[SaveSlotCount];
			var hasExisting = false;
			for (var i = 0; i < SaveSlotCount; i++)
			{
				options[i] = CreateSaveSlotOption(i);
				if (File.Exists(GetSaveFilePath(i))) hasExisting = true;
			}
			if (requireExisting && !hasExisting)
			{
				await DialogueManager.ShowGenericDialogue("当前没有可读取的存档");
				return null;
			}
			var menu = DialogueManager.CreateMenuDialogue("选择存档槽", !requireExisting, options);
			var choice = await menu;
			if (choice == options.Length) return null;
			var saveFilePath = GetSaveFilePath(choice);
			var exists = File.Exists(saveFilePath);
			switch (requireExisting)
			{
				case true when !exists:
				{
					await DialogueManager.ShowGenericDialogue("该槽位暂无存档");
					continue;
				}
				case false when exists:
				{
					var confirmMenu = DialogueManager.CreateMenuDialogue(
						"确认操作",
						true,
						new MenuOption { title = "覆盖存档", description = "开始新游戏将覆盖该槽位", }
					);
					var confirmChoice = await confirmMenu;
					if (confirmChoice == 0) return choice;
					continue;
				}
				default:
					return choice;
			}
		}
	}
	public override void _Ready()
	{
		Log.Print("[ProgramRoot] 程序启动");
		var godotPath = Settings.Get("godot");
		if (godotPath != null) Log.Print($"[ProgramRoot] 从配置读取godot路径: {godotPath}");
		if (LaunchArgs.port.HasValue)
			AddChild(new CommandHandler(this));
		else
			StartGameLoop();
	}
	public async void StartGameLoop()
	{
		try
		{
			while (this.Valid())
				try
				{
					if (!await Routine()) break;
				}
				catch (Exception e)
				{
					Log.PrintException(e);
				}
			await Task.Delay(1);
			GetTree().Quit();
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
	async Task<bool> Routine()
	{
		var menu = DialogueManager.CreateMenuDialogue(
			"主菜单",
			new MenuOption { title = "开始游戏", description = "开始新的冒险", },
			new MenuOption { title = "读取游戏", description = "读取冒险", },
			new MenuOption { title = "退出游戏", description = "关闭游戏程序", }
		);
		var choice = await menu;
		switch (choice)
		{
			case 0:
			{
				var slotIndex = await SelectSaveSlot(false);
				if (slotIndex is null) break;
				var filePath = GetSaveFilePath(slotIndex.Value);
				await RunNewGame(filePath);
				break;
			}
			case 1:
			{
				var slotIndex = await SelectSaveSlot(true);
				if (slotIndex is null) break;
				var filePath = GetSaveFilePath(slotIndex.Value);
				await RunLoadedGame(filePath);
				break;
			}
			case 2:
			{
				Log.Print("游戏即将退出...");
				GameServer.McpCheckpoint();
				await Task.Delay(1);
				GetTree().Quit();
				return false;
			}
			default:
			{
				throw new InvalidOperationException("未知的菜单选项");
			}
		}
		return true;
	}
	/// <summary>
	///     运行新的游戏流程。
	/// </summary>
	async Task RunNewGame(string saveFilePath)
	{
		PackedScene gameNodeScene = ResourceTable.gameNodeScene;
		var gameNode = gameNodeScene.Instantiate();
		AddChild(gameNode);
		var game = new Game(saveFilePath, gameNode);
		try
		{
			await game;
		}
		finally
		{
			gameNode.QueueFree();
		}
	}
	/// <summary>
	///     读取存档并运行游戏流程。
	/// </summary>
	async Task RunLoadedGame(string saveFilePath)
	{
		if (!File.Exists(saveFilePath))
		{
			await DialogueManager.ShowGenericDialogue("存档文件不存在");
			return;
		}
		byte[] saveData;
		await using (var stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			await using var memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream);
			saveData = memoryStream.ToArray();
		}
		using var reader = new BinaryReader(new MemoryStream(saveData));
		PackedScene gameNodeScene = ResourceTable.gameNodeScene;
		var gameNode = gameNodeScene.Instantiate();
		AddChild(gameNode);
		var game = new Game(saveFilePath, reader, gameNode);
		try
		{
			await game;
		}
		finally
		{
			gameNode.QueueFree();
		}
	}
}
