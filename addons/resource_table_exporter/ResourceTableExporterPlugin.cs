#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Environment = System.Environment;
namespace RealismCombat.addons.resource_table_exporter;
[Tool]
public partial class ResourceTableExporterPlugin : EditorPlugin
{
	class SceneInfo
	{
		public string ConstantName { get; set; } = "";
		public string GodotPath { get; init; } = "";
	}
	class AudioInfo
	{
		public string ConstantName { get; set; } = "";
		public string GodotPath { get; init; } = "";
	}
	const string resourceTablePath = "Scripts/ResourceTable.cs";
	const string scenesDirectory = "res://Scenes/";
	const string audiosDirectory = "res://Audios/";
	const string autogenRegionName = "autogen_scenes";
	const string autogenAudiosRegionName = "autogen_audios";
	static readonly string[] audioExtensions = { ".mp3", ".ogg", ".wav" };
	static void ResolveNameConflicts(List<SceneInfo> sceneFiles)
	{
		var nameCount = new Dictionary<string, int>();
		var conflictedNames = new HashSet<string>();
		foreach (var scene in sceneFiles)
			if (nameCount.TryGetValue(key: scene.ConstantName, value: out var value))
			{
				nameCount[scene.ConstantName] = ++value;
				conflictedNames.Add(scene.ConstantName);
			}
			else
			{
				nameCount[scene.ConstantName] = 1;
			}
		if (conflictedNames.Count > 0)
		{
			var nameCounters = new Dictionary<string, int>();
			foreach (var conflictName in conflictedNames) nameCounters[conflictName] = 1;
			foreach (var scene in sceneFiles)
				if (conflictedNames.Contains(scene.ConstantName))
				{
					var counter = nameCounters[scene.ConstantName];
					scene.ConstantName = $"{scene.ConstantName}{counter}";
					nameCounters[scene.ConstantName.Replace(oldValue: counter.ToString(), newValue: "")] = counter + 1;
				}
			GD.PrintErr($"[ResourceTableExporter] 警告: 发现 {conflictedNames.Count} 个命名冲突，已自动添加数字后缀");
		}
	}
	static void ResolveNameConflicts(List<AudioInfo> audioFiles)
	{
		var nameCount = new Dictionary<string, int>();
		var conflictedNames = new HashSet<string>();
		foreach (var audio in audioFiles)
			if (nameCount.TryGetValue(key: audio.ConstantName, value: out var value))
			{
				nameCount[audio.ConstantName] = ++value;
				conflictedNames.Add(audio.ConstantName);
			}
			else
			{
				nameCount[audio.ConstantName] = 1;
			}
		if (conflictedNames.Count > 0)
		{
			var nameCounters = new Dictionary<string, int>();
			foreach (var conflictName in conflictedNames) nameCounters[conflictName] = 1;
			foreach (var audio in audioFiles)
				if (conflictedNames.Contains(audio.ConstantName))
				{
					var counter = nameCounters[audio.ConstantName];
					audio.ConstantName = $"{audio.ConstantName}{counter}";
					nameCounters[audio.ConstantName.Replace(oldValue: counter.ToString(), newValue: "")] = counter + 1;
				}
			GD.PrintErr($"[ResourceTableExporter] 警告: 发现 {conflictedNames.Count} 个音频命名冲突，已自动添加数字后缀");
		}
	}
	static string GenerateConstantName(string godotPath, string baseDirectory)
	{
		var relativePath = godotPath.Replace(oldValue: baseDirectory, newValue: "");
		var parts = relativePath.Split('/');
		var nameBuilder = new StringBuilder();
		for (var i = 0; i < parts.Length; i++)
		{
			var part = parts[i];
			if (string.IsNullOrWhiteSpace(part)) continue;
			if (i == parts.Length - 1) part = Path.GetFileNameWithoutExtension(part);
			var sanitized = SanitizeIdentifier(part);
			if (!string.IsNullOrEmpty(sanitized)) nameBuilder.Append(ToCamelCase(sanitized));
		}
		var result = nameBuilder.ToString();
		if (string.IsNullOrEmpty(result)) result = "unnamed";
		if (result.Length > 0 && char.IsUpper(result[0])) result = char.ToLower(result[0]) + result.Substring(1);
		if (char.IsDigit(result[0])) result = "_" + result;
		return result;
	}
	static string SanitizeIdentifier(string str)
	{
		if (string.IsNullOrWhiteSpace(str)) return "";
		str = Regex.Replace(input: str, pattern: @"[^a-zA-Z0-9_]", replacement: "");
		return str;
	}
	static string ToCamelCase(string str)
	{
		if (string.IsNullOrWhiteSpace(str)) return str;
		var result = new StringBuilder();
		var capitalizeNext = true;
		foreach (var c in str)
			if (char.IsLetterOrDigit(c))
			{
				if (capitalizeNext)
				{
					result.Append(char.ToUpper(c));
					capitalizeNext = false;
				}
				else
				{
					result.Append(char.ToLower(c));
				}
			}
			else
			{
				capitalizeNext = true;
			}
		return result.ToString();
	}
	static string GenerateConstants(List<SceneInfo> sceneFiles)
	{
		var sb = new StringBuilder();
		foreach (var scene in sceneFiles) sb.AppendLine($"\tpublic const string {scene.ConstantName} = \"{scene.GodotPath}\";");
		if (sb.Length > 0) sb.Length -= Environment.NewLine.Length;
		return sb.ToString();
	}
	static string GenerateAudioConstants(List<AudioInfo> audioFiles)
	{
		var sb = new StringBuilder();
		foreach (var audio in audioFiles) sb.AppendLine($"\tpublic const string {audio.ConstantName} = \"{audio.GodotPath}\";");
		if (sb.Length > 0) sb.Length -= Environment.NewLine.Length;
		return sb.ToString();
	}
	static void UpdateResourceTable(string filePath, string generated)
	{
		var content = File.ReadAllText(filePath);
		const string regionPattern = $@"#region {autogenRegionName}.*?#endregion {autogenRegionName}";
		var regex = new Regex(pattern: regionPattern, options: RegexOptions.Singleline);
		var replacement = $"#region {autogenRegionName}\n{generated}\n\t#endregion {autogenRegionName}";
		if (regex.IsMatch(content))
			content = regex.Replace(input: content, replacement: replacement);
		else
			throw new InvalidOperationException($"找不到 #region {autogenRegionName}");
		File.WriteAllText(path: filePath, contents: content);
	}
	static void UpdateAudioTable(string filePath, string generated)
	{
		var content = File.ReadAllText(filePath);
		const string regionPattern = $@"#region {autogenAudiosRegionName}.*?#endregion {autogenAudiosRegionName}";
		var regex = new Regex(pattern: regionPattern, options: RegexOptions.Singleline);
		var replacement = $"#region {autogenAudiosRegionName}\n{generated}\n\t#endregion {autogenAudiosRegionName}";
		if (regex.IsMatch(content))
			content = regex.Replace(input: content, replacement: replacement);
		else
			throw new InvalidOperationException($"找不到 #region {autogenAudiosRegionName}");
		File.WriteAllText(path: filePath, contents: content);
	}
	Button? toolbarButton;
	public override void _EnterTree()
	{
		toolbarButton = new()
		{
			Text = "生成ResourceTable",
		};
		toolbarButton.Pressed += OnGeneratePressed;
		AddControlToContainer(container: CustomControlContainer.Toolbar, control: toolbarButton);
		GD.Print("[ResourceTableExporter] 插件已加载");
	}
	public override void _ExitTree()
	{
		if (toolbarButton != null)
		{
			RemoveControlFromContainer(container: CustomControlContainer.Toolbar, control: toolbarButton);
			toolbarButton.QueueFree();
			toolbarButton = null;
		}
		GD.Print("[ResourceTableExporter] 插件已卸载");
	}
	void OnGeneratePressed()
	{
		try
		{
			GenerateResourceTable();
			GD.Print("[ResourceTableExporter] 成功生成ResourceTable常量");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[ResourceTableExporter] 生成失败: {ex.Message}");
		}
	}
	void GenerateResourceTable()
	{
		var projectRoot = ProjectSettings.GlobalizePath("res://");
		var resourceTableFile = Path.Combine(path1: projectRoot, path2: resourceTablePath);
		if (!File.Exists(resourceTableFile)) throw new FileNotFoundException($"找不到文件: {resourceTableFile}");
		var sceneFiles = CollectSceneFiles();
		var generated = GenerateConstants(sceneFiles);
		UpdateResourceTable(filePath: resourceTableFile, generated: generated);
		GD.Print($"[ResourceTableExporter] 已生成 {sceneFiles.Count} 个场景常量");
		var audioFiles = CollectAudioFiles();
		var audioGenerated = GenerateAudioConstants(audioFiles);
		UpdateAudioTable(filePath: resourceTableFile, generated: audioGenerated);
		GD.Print($"[ResourceTableExporter] 已生成 {audioFiles.Count} 个音频常量");
	}
	List<SceneInfo> CollectSceneFiles()
	{
		var sceneFiles = new List<SceneInfo>();
		CollectSceneFilesRecursive(dirPath: scenesDirectory, sceneFiles: sceneFiles);
		ResolveNameConflicts(sceneFiles);
		return sceneFiles.OrderBy(s => s.ConstantName).ToList();
	}
	void CollectSceneFilesRecursive(string dirPath, List<SceneInfo> sceneFiles)
	{
		var dir = DirAccess.Open(dirPath);
		if (dir == null)
		{
			GD.PrintErr($"[ResourceTableExporter] 无法打开目录: {dirPath}");
			return;
		}
		dir.ListDirBegin();
		var fileName = dir.GetNext();
		while (!string.IsNullOrEmpty(fileName))
		{
			if (fileName is "." or "..")
			{
				fileName = dir.GetNext();
				continue;
			}
			var fullPath = dirPath + fileName;
			if (dir.CurrentIsDir())
			{
				CollectSceneFilesRecursive(dirPath: fullPath + "/", sceneFiles: sceneFiles);
			}
			else if (fileName.EndsWith(".tscn"))
			{
				var constantName = GenerateConstantName(fullPath, scenesDirectory);
				sceneFiles.Add(new()
				{
					ConstantName = constantName,
					GodotPath = fullPath,
				});
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
	}
	List<AudioInfo> CollectAudioFiles()
	{
		var audioFiles = new List<AudioInfo>();
		CollectAudioFilesRecursive(dirPath: audiosDirectory, audioFiles: audioFiles);
		ResolveNameConflicts(audioFiles);
		return audioFiles.OrderBy(s => s.ConstantName).ToList();
	}
	void CollectAudioFilesRecursive(string dirPath, List<AudioInfo> audioFiles)
	{
		var dir = DirAccess.Open(dirPath);
		if (dir == null)
		{
			GD.PrintErr($"[ResourceTableExporter] 无法打开目录: {dirPath}");
			return;
		}
		dir.ListDirBegin();
		var fileName = dir.GetNext();
		while (!string.IsNullOrEmpty(fileName))
		{
			if (fileName is "." or "..")
			{
				fileName = dir.GetNext();
				continue;
			}
			var fullPath = dirPath + fileName;
			if (dir.CurrentIsDir())
			{
				CollectAudioFilesRecursive(dirPath: fullPath + "/", audioFiles: audioFiles);
			}
			else if (IsAudioFile(fileName))
			{
				var constantName = GenerateConstantName(fullPath, audiosDirectory);
				audioFiles.Add(new()
				{
					ConstantName = constantName,
					GodotPath = fullPath,
				});
			}
			fileName = dir.GetNext();
		}
		dir.ListDirEnd();
	}
	bool IsAudioFile(string fileName)
	{
		foreach (var ext in audioExtensions)
			if (fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
				return true;
		return false;
	}
}
#endif
