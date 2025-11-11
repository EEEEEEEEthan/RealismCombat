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
	static ShaderMaterial? jumpMaterial;
	static Shader? jumpShader;
	static ShaderMaterial JumpMaterial
	{
		get
		{
			var material = jumpMaterial;
			if (material == null)
			{
				var shader = jumpShader;
				if (shader == null)
				{
					shader = new();
					shader.Code = JumpShaderSource;
					jumpShader = shader;
				}
				material = new();
				material.Shader = shader;
				material.SetShaderParameter("interval", 0.15);
				jumpMaterial = material;
			}
			return material;
		}
	}
	string title = null!;
	double current;
	double max;
	bool jump;
	Label? label;
	ProgressBar? progressBar;
	public Label Label => label ??= GetNodeOrNull<Label>("Label");
	public ProgressBar ProgressBar => progressBar ??= GetNodeOrNull<ProgressBar>("ProgressBar");
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
	public double Progress => Max == 0 ? 0 : Current / Max;
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
	void UpdateTitle() => Label?.Text = title;
	void UpdateValue()
	{
		ProgressBar?.MaxValue = Max;
		ProgressBar?.Value = Current;
	}
	void UpdateJump() => ProgressBar?.Material = jump ? JumpMaterial : null;
}
