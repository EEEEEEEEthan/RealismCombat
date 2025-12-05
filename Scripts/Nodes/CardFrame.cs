using System.Diagnostics.CodeAnalysis;
using Godot;
public partial class CardFrame : Control
{
	const float FlashDuration = 0.2f;
	Tween? flashTween;
	public Color Color
	{
		get;
		set
		{
			field = value;
			Background?.SelfModulate = value;
		}
	}
	public bool Bleeding
	{
		get;
		set
		{
			field = value;
			BleedingNode?.Visible = value;
		}
	}
	[field: MaybeNull,] Control Background => field ??= GetNode<Control>("%Background");
	[field: MaybeNull,] Control BleedingNode => field ??= GetNode<Control>("%Bleeding");
	[field: MaybeNull,] Control FlashNode => field ??= GetNode<Control>("%Flash");
	[field: MaybeNull,] Control FlashContent => field ??= FlashNode.GetNode<Control>("FlashContent");
	public override void _Ready()
	{
		base._Ready();
		Background?.SelfModulate = Color;
		BleedingNode?.Visible = Bleeding;
	}
	public void Flash()
	{
		flashTween?.Kill();
		var parentWidth = Background.Size.X;
		var flashContentRect = FlashContent.GetRect();
		var flashContentWidth = flashContentRect.Size.X;
		var startX = -flashContentWidth;
		var endX = parentWidth + flashContentWidth;
		FlashContent.Position = new(startX, FlashContent.Position.Y);
		flashTween = FlashContent.CreateTween();
		flashTween.TweenProperty(FlashContent, "position:x", endX, FlashDuration);
	}
}
