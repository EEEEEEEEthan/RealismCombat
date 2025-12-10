using Godot;
[Tool]
public partial class CardFrame : Control
{
	const float flashDuration = 0.2f;
	Tween? flashTween;
	[Export]
	public Color Color
	{
		get;
		set
		{
			field = value;
			Background?.SelfModulate = value;
		}
	}
	[Export]
	public bool Bleeding
	{
		get;
		set
		{
			field = value;
			BleedingNode?.Visible = value;
		}
	}
	Control? Background => field ??= GetNode<Control>("%Background");
	Control? BleedingNode => field ??= GetNode<Control>("%Bleeding");
	Control? FlashNode => field ??= GetNode<Control>("%Flash");
	Control? FlashContent => field ??= FlashNode?.GetNode<Control>("FlashContent");
	public override void _Ready()
	{
		base._Ready();
		Background?.SelfModulate = Color;
		BleedingNode?.Visible = Bleeding;
	}
	public void Flash()
	{
		flashTween?.Kill();
		var parentWidth = Background!.Size.X;
		var flashContentRect = FlashContent!.GetRect();
		var flashContentWidth = flashContentRect.Size.X;
		var startX = -flashContentWidth;
		var endX = parentWidth + flashContentWidth;
		FlashContent.Position = new(startX, FlashContent.Position.Y);
		flashTween = FlashContent.CreateTween();
		flashTween.TweenProperty(FlashContent, "position:x", endX, flashDuration);
	}
}
