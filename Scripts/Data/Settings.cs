using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace RealismCombat.Data;
public class Settings
{
	const string settingsFilePath = ".local.settings";
	const string skipSaveKey = "SkipSave";
	public static Settings Load()
	{
		var settings = new Settings();
		if (!File.Exists(settingsFilePath)) return settings;
		try
		{
			var lines = File.ReadAllLines(path: settingsFilePath, encoding: Encoding.UTF8);
			foreach (var line in lines)
			{
				var trimmedLine = line.Trim();
				if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#')) continue;
				var parts = trimmedLine.Split(separator: '=', count: 2);
				if (parts.Length != 2) continue;
				var key = parts[0].Trim();
				var value = parts[1].Trim();
				if (key == skipSaveKey)
					if (bool.TryParse(value: value, result: out var boolValue))
						settings.SkipSave = boolValue;
			}
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
		return settings;
	}
	public bool SkipSave { get; set; }
	public Settings() => SkipSave = false;
	public void Save()
	{
		try
		{
			var existingSettings = new Dictionary<string, string>();
			if (File.Exists(settingsFilePath))
			{
				var lines = File.ReadAllLines(path: settingsFilePath, encoding: Encoding.UTF8);
				foreach (var line in lines)
				{
					var trimmedLine = line.Trim();
					if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#')) continue;
					var parts = trimmedLine.Split(separator: '=', count: 2);
					if (parts.Length != 2) continue;
					var key = parts[0].Trim();
					var value = parts[1].Trim();
					existingSettings[key] = value;
				}
			}
			existingSettings[skipSaveKey] = SkipSave.ToString();
			using var writer = new StreamWriter(path: settingsFilePath, append: false, encoding: Encoding.UTF8);
			foreach (var kvp in existingSettings) writer.WriteLine($"{kvp.Key} = {kvp.Value}");
		}
		catch (Exception e)
		{
			Log.PrintException(e);
		}
	}
}
