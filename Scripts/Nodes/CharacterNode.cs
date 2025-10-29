using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
partial class CharacterNode : Control
{
	public static CharacterNode Create()
	{
		var instance = GD.Load<PackedScene>(ResourceTable.character).Instantiate<CharacterNode>();
		return instance;
	}
	[Export] Label nameLabel = null!;
	[Export] PropertyDrawerNode actionPoint = null!;
	[Export] PropertyDrawerNode speed = null!;
	[Export] PropertyDrawerNode head = null!;
	[Export] PropertyDrawerNode chest = null!;
	[Export] PropertyDrawerNode rightArm = null!;
	[Export] PropertyDrawerNode leftArm = null!;
	[Export] PropertyDrawerNode rightLeg = null!;
	[Export] PropertyDrawerNode leftLeg = null!;
	public CharacterData? CharacterData { get; set; }
	public override void _Process(double delta)
	{
		if (CharacterData == null) return;
		nameLabel.Text = CharacterData.name;
		actionPoint.property = ((int)CharacterData.actionPoint + 10, 10);
		speed.property = ((int)CharacterData.speed, 10);
		head.property = (CharacterData.head.hp, CharacterData.head.maxHp);
		chest.property = (CharacterData.chest.hp, CharacterData.chest.maxHp);
		rightArm.property = (CharacterData.rightArm.hp, CharacterData.rightArm.maxHp);
		leftArm.property = (CharacterData.leftArm.hp, CharacterData.leftArm.maxHp);
		rightLeg.property = (CharacterData.rightLeg.hp, CharacterData.rightLeg.maxHp);
		leftLeg.property = (CharacterData.leftLeg.hp, CharacterData.leftLeg.maxHp);
		actionPoint.title = "行动";
		speed.title = "速度";
		head.title = "头部";
		chest.title = "胸部";
		rightArm.title = "右臂";
		leftArm.title = "左臂";
		rightLeg.title = "右腿";
		leftLeg.title = "左腿";
	}
}
