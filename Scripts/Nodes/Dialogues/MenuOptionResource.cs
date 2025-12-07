using Godot;

[GlobalClass]
[Tool]
public partial class MenuOptionResource : Resource
{
	[Export]
	public string text
	{
		get;
		set;
	} = string.Empty;
	[Export]
	public bool disabled
	{
		get;
		set;
	}
}

