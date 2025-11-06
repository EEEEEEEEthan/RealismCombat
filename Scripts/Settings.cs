using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;
namespace RealismCombat;
/// <summary>
///     读取和管理.local.settings配置文件
/// </summary>
public static partial class Settings
{
	static readonly Dictionary<string, string> settings = new();
	static readonly string settingsPath;
	static Settings()
	{
		var projectRoot = ProjectSettings.GlobalizePath("res://");
		settingsPath = Path.Combine(projectRoot, ".local.settings");
		LoadSettings();
	}
	public static string? Get(string key) => settings.GetValueOrDefault(key);
	public static string Get(string key, string defaultValue) => settings.GetValueOrDefault(key, defaultValue);
	static void LoadSettings()
	{
		settings.Clear();
		if (!File.Exists(settingsPath))
		{
			Log.PrintErr($"[Settings] 配置文件不存在: {settingsPath}");
			return;
		}
		var regex = ConfigRegex();
		foreach (var line in File.ReadAllLines(settingsPath))
		{
			var match = regex.Match(line);
			if (match.Success)
			{
				var key = match.Groups[1].Value.Trim();
				var value = match.Groups[2].Value.Trim();
				settings[key] = value;
				Log.Print($"[Settings] 加载配置: {key} = {value}");
			}
		}
	}
	[GeneratedRegex(@"(\S+)\s*=\s*(.+)")] private static partial Regex ConfigRegex();
}
