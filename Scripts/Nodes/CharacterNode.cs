using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
partial class CharacterNode : Control
{
	public static CharacterNode Create()
	{
		var instance = GD.Load<PackedScene>(ResourceTable.characterScene).Instantiate<CharacterNode>();
		return instance;
	}
	[Export] RichTextLabel nameLabel = null!;
	[Export] RichTextLabel actionPointLabel = null!;
	[Export] BodyPartNode head = null!;
	[Export] BodyPartNode chest = null!;
	[Export] BodyPartNode rightArm = null!;
	[Export] BodyPartNode leftArm = null!;
	[Export] BodyPartNode rightLeg = null!;
	[Export] BodyPartNode leftLeg = null!;
	RichTextLabel richTextLabelName = null!;
	public CharacterData? CharacterData { get; set; }
	public override void _Process(double delta)
	{
		nameLabel.Text = CharacterData?.name;
		actionPointLabel.Text = $"行动点{CharacterData?.actionPoint:F0}";
		if (CharacterData != null)
		{
			head.bodyPartData = CharacterData.head;
			chest.bodyPartData = CharacterData.chest;
			rightArm.bodyPartData = CharacterData.rightArm;
			leftArm.bodyPartData = CharacterData.leftArm;
			rightLeg.bodyPartData = CharacterData.rightLeg;
			leftLeg.bodyPartData = CharacterData.leftLeg;
		}
	}
}
