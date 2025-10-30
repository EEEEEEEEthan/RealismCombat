using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes;
[Tool]
partial class PropertyDrawerNode : Node
{
	float value;
	string title;
	Label titleControl;
	Control valueControl;
	[Export]
	public string Title
	{
		get => title;
		set
		{
			if (title == value) return;
			title = value;
			UpdateProperties();
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
			UpdateProperties();
		}
	}
	Label TitleControl => titleControl ??= GetNode<Label>("Name");
	Control ValueControl => valueControl ??= GetNode<Control>("ValueContainer/Value");
	public override void _Ready() => UpdateProperties();
	public override void _Process(double delta) => UpdateProperties();
	void UpdateProperties()
	{
		if (TitleControl?.Valid() != true) return;
		if (ValueControl?.Valid() != true) return;
		TitleControl.Text = Title;
		var width = (int)value.Remapped(fromMin: 0, fromMax: 1, toMin: 0, toMax: 20);
		if (width % 2 == 0) width -= 1;
		ValueControl.CustomMinimumSize = ValueControl.CustomMinimumSize with { X = width, };
	}
}
