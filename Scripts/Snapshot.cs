using System;
using System.IO;
public record Snapshot
{
	public readonly int chapter;
	readonly GameVersion version;
	readonly DateTime savedAt;
	public GameVersion Version => version;
	public DateTime SavedAt => savedAt;
	public string Title
	{
		get
		{
			var delta = DateTime.Now - SavedAt;
			return delta.TotalMinutes switch
			{
				< 1 => "刚刚",
				< 60 => $"{(int)delta.TotalMinutes}分钟前",
				_ => delta.TotalHours switch
				{
					< 24 => $"{(int)delta.TotalHours}小时前",
					_ => delta.TotalDays switch
					{
						< 7 => $"{(int)delta.TotalDays}天前",
						< 30 => $"{(int)(delta.TotalDays / 7)}周前",
						< 365 => $"{(int)(delta.TotalDays / 30)}个月前",
						_ => $"{SavedAt:yyyy-M-d}",
					},
				},
			};
		}
	}
	public string Desc => $"version: {version}";
	public Snapshot(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			version = new(reader);
			savedAt = new DateTime(reader.ReadInt64(), DateTimeKind.Utc).ToLocalTime();
			chapter = reader.ReadInt32();
		}
	}
	public Snapshot(Game game)
	{
		chapter = game.Chapter;
		version = GameVersion.newest;
		savedAt = DateTime.Now;
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			version.Serialize(writer);
			writer.Write(savedAt.ToUniversalTime().Ticks);
			writer.Write(chapter);
		}
	}
}
