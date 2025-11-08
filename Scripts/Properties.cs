using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat;
public class PropertySingle
{
	public float value;
	public float maxValue;
	public PropertySingle(float value, float maxValue)
	{
		this.value = value;
		this.maxValue = maxValue;
	}
	public PropertySingle(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			value = reader.ReadSingle();
			maxValue = reader.ReadSingle();
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(value);
			writer.Write(maxValue);
		}
	}
}
public class PropertyInt
{
	public int value;
	public int maxValue;
	public PropertyInt(int value, int maxValue)
	{
		this.value = value;
		this.maxValue = maxValue;
	}
	public PropertyInt(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			value = reader.ReadInt32();
			maxValue = reader.ReadInt32();
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write(value);
			writer.Write(maxValue);
		}
	}
}
