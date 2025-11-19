using System.IO;
namespace RealismCombat.Items;
public class LongSword() : Item(ItemIdCode.LongSword, ItemFlagCode.Arm, [], new(10, 10)), IArm
{
	public override string Name => "长剑";
	public double Length => 100.0;
	public double Weight => 1.2;
	protected override void OnSerialize(BinaryWriter writer) { }
	protected override void OnDeserialize(BinaryReader reader) { }
}
