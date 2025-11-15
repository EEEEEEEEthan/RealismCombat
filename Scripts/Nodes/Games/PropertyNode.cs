using System.Diagnostics.CodeAnalysis;
using Godot;
namespace RealismCombat.Nodes.Games;
[Tool]
public partial class PropertyNode : Control
{
	const string JumpShaderSource = """
		shader_type canvas_item;
		#include "res://Shaders/random.gdshaderinc"
		#include "res://Shaders/utilities.gdshaderinc"

		uniform float interval : hint_range(0.0, 1.0) = 0.1;

		void fragment() {
		    vec2 uv = UV;
		    float x = floor(uv.x / TEXTURE_PIXEL_SIZE.x) + floor(TIME / interval) * interval;
		    float offset = remap(fract_random(x), 0.0, 1.0, -0.75, 0.75);
		    uv.y += TEXTURE_PIXEL_SIZE.y * round(offset);
		    COLOR = texture(TEXTURE, uv);
		}
		""";
	const float FlashDuration = 0.2f;
	static Shader? jumpShader;
	[field: AllowNull, MaybeNull,] static ShaderMaterial JumpMaterial => field ??= CreateJumpMaterial();
	static ShaderMaterial CreateJumpMaterial()
	{
		var shader = jumpShader ??= new() { Code = JumpShaderSource, };
		var material = new ShaderMaterial();
		material.Shader = shader;
		material.SetShaderParameter("interval", 0.15);
		return material;
	}
	string title = null!;
	double current;
	double max;
	bool jump;
	SceneTreeTimer? flashTimer;
	[field: AllowNull, MaybeNull,] public Label Label => field ??= GetNodeOrNull<Label>("Label");
	[field: AllowNull, MaybeNull,] public ProgressBar ProgressBar => field ??= GetNodeOrNull<ProgressBar>("ProgressBar");
	public double Progress => Max == 0 ? 0 : Current / Max;
	[Export]
	public string Title
	{
		get => title;
		set
		{
			title = value;
			UpdateTitle();
		}
	}
	public (double current, double max) Value
	{
		get => (Current, Max);
		set
		{
			Max = value.max;
			Current = value.current;
		}
	}
	[Export]
	public bool Jump
	{
		get => jump;
		set
		{
			jump = value;
			UpdateJump();
		}
	}
	[Export]
	double Current
	{
		get => current;
		set
		{
			current = value;
			UpdateValue();
		}
	}
	[Export]
	double Max
	{
		get => max;
		set
		{
			max = value;
			UpdateValue();
		}
	}
	public override void _Ready()
	{
		base._Ready();
		UpdateTitle();
		UpdateValue();
		UpdateJump();
	}
	/// <summary>
	///     闪烁红色，持续0.2秒
	/// </summary>
	public void FlashRed()
	{
		var originalModulate = Modulate;
		var flashColor = GameColors.pinkGradient[^1];
		Modulate = flashColor;
		flashTimer = GetTree().CreateTimer(FlashDuration);
		flashTimer.Timeout += () =>
		{
			Modulate = originalModulate;
			flashTimer = null;
		};
	}
	void UpdateTitle() => Label?.Text = title;
	void UpdateValue()
	{
		ProgressBar?.MaxValue = Max;
		ProgressBar?.Value = Current;
	}
	void UpdateJump() => ProgressBar?.Material = jump ? JumpMaterial : null;
}
