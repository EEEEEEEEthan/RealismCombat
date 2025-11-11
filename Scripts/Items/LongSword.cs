using System.IO;
namespace RealismCombat.Items;
public class LongSword() : Item(ItemIdCode.LongSword, ItemFlagCode.Arm, [], new(10, 10))
{
	public override string Name => "长剑";
	protected override void OnSerialize(BinaryWriter writer) { }
	protected override void OnDeserialize(BinaryReader reader) { }
}
