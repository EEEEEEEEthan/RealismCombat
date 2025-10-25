using Godot;
namespace RealismCombat;
public partial class BattlePrepareScene : Node
{
	public static BattlePrepareScene Create() => GD.Load<PackedScene>(ResourceTable.battlePrepareScene).Instantiate<BattlePrepareScene>();
}
