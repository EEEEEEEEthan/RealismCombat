using Godot;
namespace RealismCombat.Scenes;
[Tool, GlobalClass,]
public partial class Printer : RichTextLabel
{
	[Export] public float interval = 0.1f;
	float timer;
	public override bool _Set(StringName property, Variant value)
	{
		if (property == "text")
		{
			VisibleCharacters = 0;
			UpdateVisibleCharacters();
		}
		return base._Set(property, value);
	}
	public override void _Process(double delta)
	{
		timer += (float)delta;
		if (timer >= interval)
		{
			VisibleCharacters += 1;
			timer = 0;
		}
		UpdateVisibleCharacters();
	}
	void UpdateVisibleCharacters() => VisibleCharacters = Mathf.Min(VisibleCharacters, GetTotalCharacterCount());
}
