using Godot;

[GlobalClass]
[Tool]
public partial class MenuOptionResource : Resource
{
	[Export]
	public string Text
	{
		get;
		set;
	} = string.Empty;
	[Export]
	public bool Disabled
	{
		get;
		set;
	}
}

