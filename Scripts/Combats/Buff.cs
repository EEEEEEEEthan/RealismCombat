using System.IO;
using RealismCombat.Extensions;
namespace RealismCombat.Combats;
/// <summary>
///     Buff类
/// </summary>
public class Buff
{
	public readonly BuffCode code;
	public Buff(BuffCode code)
	{
		this.code = code;
	}
	public Buff(BinaryReader reader)
	{
		using (reader.ReadScope())
		{
			code = (BuffCode)reader.ReadUInt64();
		}
	}
	public void Serialize(BinaryWriter writer)
	{
		using (writer.WriteScope())
		{
			writer.Write((ulong)code);
		}
	}
}

