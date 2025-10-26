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
	RichTextLabel richTextLabelName = null!;
	public CharacterData? CharacterData { get; set; }
	public override void _Process(double delta)
	{
		nameLabel.Text = CharacterData?.name;
		actionPointLabel.Text = $"行动点{CharacterData?.actionPoint:F0}";
	}
}
