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
	PanelContainer panelContainer = null!;
	Label nameLabel = null!;
	PropertyDrawerNode actionPoint = null!;
	PropertyDrawerNode speed = null!;
	PropertyDrawerNode head = null!;
	PropertyDrawerNode chest = null!;
	PropertyDrawerNode rightArm = null!;
	PropertyDrawerNode leftArm = null!;
	PropertyDrawerNode rightLeg = null!;
	PropertyDrawerNode leftLeg = null!;
	public CharacterData? CharacterData { get; set; }
	public override void _Ready()
	{
		panelContainer = GetNode<PanelContainer>("PanelContainer");
		nameLabel = GetNode<Label>("PanelContainer/VBoxContainer/Title");
		actionPoint = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/ActionPoint");
		speed = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Speed");
		head = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Head");
		chest = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/Chest");
		rightArm = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/RightArm");
		leftArm = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/LeftArm");
		rightLeg = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/RightLeg");
		leftLeg = GetNode<PropertyDrawerNode>("PanelContainer/VBoxContainer/LeftLeg");
	}
	public override void _Process(double delta)
	{
		if (CharacterData == null) return;
		panelContainer.ThemeTypeVariation = CharacterData.team == 0 ? "Team0" : "Team1";
		nameLabel.Text = CharacterData.name;
		actionPoint.Value = ((float)CharacterData.ActionPoint + 10) / 10;
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
