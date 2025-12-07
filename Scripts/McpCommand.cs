using System.Collections.Generic;
using System.Linq;
using System.Text;
public readonly struct McpCommand
{
	public static McpCommand Deserialize(string data)
	{
		var parts = data.Split(' ');
		if (parts.Length == 0) return new(string.Empty);
		var command = parts[0];
		if (parts.Length == 1) return new(command);
		if ((parts.Length - 1) % 2 != 0)
		{
			Log.PrintError($"[McpCommand] 参数数量不匹配: {data}");
			return new(command);
		}
		var args = new Dictionary<string, string>();
		for (var i = 1; i < parts.Length; i += 2) args[parts[i]] = parts[i + 1];
		return new(command, args);
	}
	public string DebugMessage => $"Command: {Command}, Args: {string.Join(",", Args.Select(pair => $"{pair.Key}={pair.Value}"))}";
	public string Command { get; init; }
	public IReadOnlyDictionary<string, string> Args { get; init; }
	public McpCommand(string command, IReadOnlyDictionary<string, string>? args = null)
	{
		Command = command;
		Args = args ?? new Dictionary<string, string>();
	}
	public string Serialize()
	{
		if (Args.Count == 0) return Command;
		var sb = new StringBuilder();
		sb.Append(Command);
		foreach ((var key, var value) in Args)
		{
			sb.Append(' ');
			sb.Append(key);
			sb.Append(' ');
			sb.Append(value);
		}
		return sb.ToString();
	}
	public bool TryGetArg(string key, out string value) => Args.TryGetValue(key, out value);
	public string GetArgOrDefault(string key, string defaultValue = "") => Args.TryGetValue(key, out var value) ? value : defaultValue;
	public override string ToString() => Serialize();
}
