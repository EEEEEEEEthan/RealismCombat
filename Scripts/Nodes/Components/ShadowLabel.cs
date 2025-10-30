using Godot;
namespace RealismCombat.Nodes.Components;
[Tool, GlobalClass,]
public partial class ShadowLabel : Label
{
	Label? shadow;
	[Export]
	Label Shadow
	{
		get
		{
			shadow ??= FindChild("Foreground") as Label;
			if (shadow is null)
			{
				shadow = new();
				AddChild(node: shadow, forceReadableName: true, @internal: InternalMode.Disabled);
				shadow.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
				shadow.Position = new(x: 0, y: 1);
				shadow.ShowBehindParent = true;
				shadow.SelfModulate = new(r: 0, g: 0, b: 0, a: 1);
			}
			return shadow;
		}
		set => shadow = value;
	}
	public override bool _Set(StringName property, Variant value)
	{
		var result = base._Set(property, value);
		var propertyStr = property.ToString();
		if (propertyStr == "text" || propertyStr == "theme" || propertyStr == "theme_type_variation")
		{
			SyncProperties();
		}
		else if (propertyStr.StartsWith("theme_override_"))
		{
			SyncThemeOverrideProperty(propertyStr, value);
		}
		return result;
	}
	string lastText = "";
	public override void _Ready()
	{
		base._Ready();
		SyncProperties();
		lastText = Text;
	}
	public override void _Process(double delta)
	{
		if (Text != lastText)
		{
			lastText = Text;
			SyncText();
		}
	}
	public override void _Notification(int what)
	{
		base._Notification(what);
		if (what == NotificationThemeChanged)
		{
			SyncProperties();
		}
	}
	void SyncProperties()
	{
		var shadowLabel = Shadow;
		shadowLabel.Text = Text;
		shadowLabel.Theme = Theme;
		shadowLabel.ThemeTypeVariation = ThemeTypeVariation;
		lastText = Text;
	}
	void SyncText()
	{
		var shadowLabel = Shadow;
		shadowLabel.Text = Text;
	}
	void SyncThemeOverrideProperty(string propertyName, Variant value)
	{
		var shadowLabel = Shadow;
		shadowLabel._Set(propertyName, value);
	}
}
