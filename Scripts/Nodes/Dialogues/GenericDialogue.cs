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
	TaskCompletionSource<bool>? taskCompletionSource;
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
		if (texts.Count == 0)
		{
			currentTextIndex = -1;
		}
		else
		{
			currentTextIndex = 0;
			printerNode.Text = texts[0];
			Log.Print(printerNode.Text);
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
		if (LaunchArgs.port != null)
		{
			printerNode.interval = 0;
			TryNext();
		}
	}
	public override void HandleInput(InputEvent @event)
	{
		if (@event.IsPressed() && !@event.IsEcho()) TryNext();
	}
	public TaskAwaiter<bool> GetAwaiter()
	{
		taskCompletionSource ??= new();
		return taskCompletionSource.Task.GetAwaiter();
	}
	public void SetResult()
	{
		taskCompletionSource ??= new();
		taskCompletionSource.TrySetResult(true);
	}
	public void Reset() => taskCompletionSource = new();
	void TryNext()
	{
		if (printerNode.Printing) return;
		currentTextIndex++;
		if (currentTextIndex < texts.Count)
		{
			var txt = texts[currentTextIndex];
			printerNode.Text += "\n" + txt;
			Log.Print(txt);
		}
		else
		{
			taskCompletionSource?.TrySetResult(true);
		}
	}
}
