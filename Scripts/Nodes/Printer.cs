using Godot;
namespace RealismCombat.Nodes;
[Tool, GlobalClass,]
public partial class Printer : RichTextLabel
{
	[Export] public float interval = 0.1f;
	float timer;
	public bool Printing => VisibleCharacters < GetTotalCharacterCount();
	public Printer()
	{
		ScrollActive = false;
		ScrollFollowing = true;
		ScrollFollowingVisibleCharacters = true;
	}
	public override bool _Set(StringName property, Variant value)
	{
		if (property == "text")
		{
			var result = base._Set(property, value);
			UpdateVisibleCharacters();
			return result;
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
