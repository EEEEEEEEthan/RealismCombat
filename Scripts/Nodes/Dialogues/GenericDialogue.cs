using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Nodes.Dialogues;
[Tool, GlobalClass,]
public partial class GenericDialogue : BaseDialogue
{
	readonly List<string> texts = [];
	int currentTextIndex = -1;
	PrinterNode printerNode;
	TextureRect icon;
	VBoxContainer container;
	double time;
	TaskCompletionSource<bool>? _taskCompletionSource;
	public GenericDialogue()
	{
		container = new();
		container.Name = "VBoxContainer";
		AddChild(container);
		{
			printerNode = new();
			container.AddChild(printerNode);
			printerNode.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
		{
			icon = new();
			icon.Name = "Icon";
			container.AddChild(icon);
			icon.Texture = SpriteTable.arrowDown;
			icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		}
		CustomMinimumSize = new(128, 96);
		SetAnchorsPreset(LayoutPreset.BottomWide);
		SetOffset(Side.Left, 0);
		SetOffset(Side.Right, 0);
		SetOffset(Side.Bottom, 0);
		SetOffset(Side.Top, -CustomMinimumSize.Y);
	}
	public void SetTexts(IEnumerable<string> newTexts)
	{
		texts.Clear();
		texts.AddRange(newTexts);
		printerNode.VisibleCharacters = 0;
		if (texts.Count < 0)
		{
			currentTextIndex = -1;
		}
		else
		{
			currentTextIndex = 0;
			printerNode.Text = texts[0];
		}
	}
	public void SetText(string text) => SetTexts([text,]);
	public override void _Process(double delta)
	{
		if (Input.IsAnythingPressed())
			printerNode.interval = 0;
		else
			printerNode.interval = 0.1f;
		if (printerNode.Printing || string.IsNullOrEmpty(printerNode.Text))
		{
			icon.SelfModulate = Colors.Transparent;
		}
		else
		{
			time += delta;
			icon.SelfModulate = time > 0.5 ? Colors.White : Colors.Transparent;
			if (time > 1) time = 0;
		}
	}
	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsPressed() && !@event.IsEcho() && !printerNode.Printing)
		{
			currentTextIndex++;
			if (currentTextIndex < texts.Count)
				printerNode.Text += "\n" + texts[currentTextIndex];
			else
				_taskCompletionSource?.TrySetResult(true);
		}
	}
	public TaskAwaiter<bool> GetAwaiter()
	{
		_taskCompletionSource ??= new();
		return _taskCompletionSource.Task.GetAwaiter();
	}
	public void SetResult()
	{
		_taskCompletionSource ??= new();
		_taskCompletionSource.TrySetResult(true);
	}
	public void Reset() => _taskCompletionSource = new();
}
