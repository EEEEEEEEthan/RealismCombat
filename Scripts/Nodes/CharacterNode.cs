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
	RichTextLabel richTextLabelName = null!;
	CharacterData? characterData;
	public CharacterData? CharacterData
	{
		get => characterData;
		set
		{
			characterData = value;
			if (value == null) return;
			if (richTextLabelName != null)
				richTextLabelName.Text = value.name;
		}
	}
	public override void _Ready()
	{
		richTextLabelName = GetNode<RichTextLabel>("ColorRectBackground/RichTextLabelName");
		if (characterData != null)
			richTextLabelName.Text = characterData.name;
	}
}
