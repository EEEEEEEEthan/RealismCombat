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
		actionPoint.Value = ((float)CharacterData.actionPoint + 10) / 10;
		speed.Value = (float)CharacterData.speed / 10;
		head.Value = (float)CharacterData.head.hp / CharacterData.head.maxHp;
		chest.Value = (float)CharacterData.chest.hp / CharacterData.chest.maxHp;
		rightArm.Value = (float)CharacterData.rightArm.hp / CharacterData.rightArm.maxHp;
		leftArm.Value = (float)CharacterData.leftArm.hp / CharacterData.leftArm.maxHp;
		rightLeg.Value = (float)CharacterData.rightLeg.hp / CharacterData.rightLeg.maxHp;
		leftLeg.Value = (float)CharacterData.leftLeg.hp / CharacterData.leftLeg.maxHp;
		actionPoint.Title = "行动";
		speed.Title = "速度";
		head.Title = "头部";
		chest.Title = "胸部";
		rightArm.Title = "右臂";
		leftArm.Title = "左臂";
		rightLeg.Title = "右腿";
		leftLeg.Title = "左腿";
	}
}
