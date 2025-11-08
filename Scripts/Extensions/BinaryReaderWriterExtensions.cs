using System;
using System.IO;
namespace RealismCombat.Extensions;
public static partial class Extensions
{
	public readonly struct ReaderBlock : IDisposable
	{
		readonly BinaryReader reader;
		readonly long endPosition;
		public ReaderBlock(BinaryReader reader)
		{
			this.reader = reader;
			int length;
			try
			{
				length = reader.ReadInt32();
			}
			catch (EndOfStreamException)
			{
				Log.PrintError("检查之前的读逻辑有没有问题");
				throw;
			}
			endPosition = reader.BaseStream.Position + length;
		}
		public void Dispose() => reader.BaseStream.Position = endPosition;
	}
	public readonly struct WriterBlock : IDisposable
	{
		readonly BinaryWriter writer;
		readonly long beginPosition;
		public WriterBlock(BinaryWriter writer)
		{
			this.writer = writer;
			beginPosition = writer.BaseStream.Position;
			writer.Write(0);
		}
		public void Dispose()
		{
			var currentPosition = writer.BaseStream.Position;
			writer.BaseStream.Position = beginPosition;
			var length = (int)(currentPosition - beginPosition - sizeof(int));
			writer.Write(length);
			writer.BaseStream.Position = currentPosition;
		}
	}
	public static ReaderBlock ReadScope(this BinaryReader @this) => new(@this);
	public static ReaderBlock ReadScope(this BinaryReader @this, string key)
	{
		var disposable = new ReaderBlock(@this);
		var readKey = @this.ReadInt32();
		if (key.GetHashCode() != readKey) throw new($"Key mismatch: expected:{key}, got:{readKey}");
		return disposable;
	}
	public static ReaderBlock ReadScope(this BinaryReader @this, int key)
	{
		var disposable = new ReaderBlock(@this);
		var k = @this.ReadInt32();
		if (key != k) throw new($"Key mismatch: expected:{key}, got:{k}");
		return disposable;
	}
	public static WriterBlock WriteScope(this BinaryWriter @this) => new(@this);
	public static WriterBlock WriteScope(this BinaryWriter @this, string key)
	{
		var disposable = new WriterBlock(@this);
		@this.Write(key.GetHashCode());
		return disposable;
	}
	public static WriterBlock WriteScope(this BinaryWriter @this, int key)
	{
		var disposable = new WriterBlock(@this);
		@this.Write(key);
		return disposable;
	}
}
