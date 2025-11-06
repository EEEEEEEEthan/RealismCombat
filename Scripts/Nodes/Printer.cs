using Godot;
namespace RealismCombat.Nodes;
[Tool, GlobalClass,]
public partial class Printer : RichTextLabel
{
	[Export] public float interval = 0.1f;
	[Export] public bool enableTypingSound = true;
	float timer;
	AudioStreamPlayer? fallbackAudioPlayer;
	public bool Printing => VisibleCharacters < GetTotalCharacterCount();
	public Printer()
	{
		ScrollActive = false;
		ScrollFollowing = true;
		ScrollFollowingVisibleCharacters = true;
	}
	public override void _EnterTree()
	{
		base._EnterTree();
		Name = "Printer";
	}
	public override void _Ready()
	{
		base._Ready();
		if (!AudioManager.IsInitialized)
		{
			fallbackAudioPlayer = new()
			{
				Name = "FallbackAudioPlayer",
				Stream = ResourceTable.typingSound,
				VolumeDb = -10,
			};
			AddChild(fallbackAudioPlayer);
		}
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
			var oldVisibleChars = VisibleCharacters;
			VisibleCharacters += 1;
			if (enableTypingSound && VisibleCharacters > oldVisibleChars && VisibleCharacters <= GetTotalCharacterCount()) PlayTypingSound();
			timer = 0;
		}
		UpdateVisibleCharacters();
	}
	void PlayTypingSound()
	{
		if (fallbackAudioPlayer != null)
			fallbackAudioPlayer.Play();
		else
			AudioManager.PlaySfx(ResourceTable.typingSound, -10);
	}
	void UpdateVisibleCharacters() => VisibleCharacters = Mathf.Min(VisibleCharacters, GetTotalCharacterCount());
}
