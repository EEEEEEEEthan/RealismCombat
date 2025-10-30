using Godot;
using RealismCombat.Extensions;
using RealismCombat;
namespace RealismCombat.Nodes;
[Tool]
partial class PropertyDrawerNode : Control
{
	float value;
	string title = "属性";
	Label? titleControl;
	Control? valueControl;
	Control? valueContainer;
	float currentWidth;
	float targetWidth;
	double flashTime;
	const float flashDuration = 0.3f;
	[Export]
	public string Title
	{
		get => title;
		set
		{
			if (title == value) return;
			title = value;
			UpdateTitle();
		}
	}
	[Export(hint: PropertyHint.Range, hintString: "0,1")]
	public float Value
	{
		get => value;
		set
		{
			if (this.value == value) return;
			this.value = value;
			UpdateTargetWidth();
		}
	}
	Label? TitleControl => titleControl ??= FindChild("Title") as Label;
	Control? ValueControl => valueControl ??= FindChild("Value") as Control;
	Control? ValueContainer => valueContainer ??= FindChild("ValueContainer") as Control;
	public override void _Ready()
	{
		UpdateTitle();
		UpdateTargetWidth();
		currentWidth = 0;
		if (ValueControl?.Valid() == true)
		{
			ValueControl.CustomMinimumSize = ValueControl.CustomMinimumSize with { X = 0, };
		}
	}
	public override void _Process(double delta)
	{
		const float lerpSpeed = 10.0f;
		if (Mathf.Abs(currentWidth - targetWidth) > 0.1f)
		{
			currentWidth = Mathf.Lerp(currentWidth, targetWidth, (float)delta * lerpSpeed);
			UpdateValueWidth();
		}
		if (flashTime > 0)
		{
			flashTime -= delta;
			UpdateFlashColor();
		}
		else if (flashTime != 0)
		{
			flashTime = 0;
			UpdateFlashColor();
		}
	}
	public void Flash()
	{
		flashTime = flashDuration;
	}
	void UpdateFlashColor()
	{
		if (flashTime > 0)
		{
			Modulate = GameColors.hurtFlash;
		}
		else
		{
			Modulate = Colors.White;
		}
	}
	void UpdateTitle()
	{
		if (TitleControl?.Valid() != true) return;
		TitleControl.Text = Title;
	}
	void UpdateTargetWidth()
	{
		var width = value.Remapped(fromMin: 0, fromMax: 1, toMin: 0, toMax: 20);
		if (width % 2 == 0) width -= 1;
		targetWidth = width;
	}
	void UpdateValueWidth()
	{
		if (ValueControl?.Valid() != true) return;
		var width = (int)Mathf.Round(currentWidth);
		if (width > 0 && width % 2 == 0) width -= 1;
		if (width < 0) width = 0;
		ValueControl.CustomMinimumSize = ValueControl.CustomMinimumSize with { X = width, };
	}
}
