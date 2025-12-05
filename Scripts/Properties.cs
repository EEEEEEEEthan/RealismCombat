using System.IO;
public class PropertyDouble
{
	public double value;
	public double maxValue;
	public PropertyDouble(double value, double maxValue)
	{
		this.value = value;
		this.maxValue = maxValue;
	}
	public PropertyDouble(BinaryReader reader)
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
	public PropertyInt(BinaryReader reader) => Deserialize(reader);
	public void Deserialize(BinaryReader reader)
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
