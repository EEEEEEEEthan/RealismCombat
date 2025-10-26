using Godot;
using RealismCombat.Data;
namespace RealismCombat.Nodes;
partial class CharacterNode : Control
{
	RichTextLabel richTextLabelName = null!;
	CharacterData? characterData;
	public CharacterData? CharacterData
	{
		get => characterData;
		set
		{
			characterData = value;
			if (value == null) return;
			richTextLabelName.Text = value.name;
		}
	}
	public override void _Ready() => richTextLabelName = GetNode<RichTextLabel>("ColorRectBackground/RichTextLabelName");
}
