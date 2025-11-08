using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
namespace RealismCombat.Nodes.Dialogues;
[Tool, GlobalClass,]
public partial class GenericDialogue : BaseDialogue
{
	readonly List<string> texts = [];
	readonly TaskCompletionSource<bool> taskCompletionSource = new();
	int currentTextIndex;
	PrinterNode printerNode;
	TextureRect icon;
	VBoxContainer container;
	double time;
	public GenericDialogue(IEnumerable<string> initialTexts)
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
		texts.AddRange(initialTexts);
		printerNode.VisibleCharacters = 0;
		if (texts.Count == 0) throw new System.ArgumentException("GenericDialogue需要至少一个文本");
		currentTextIndex = 0;
		printerNode.Text = texts[0];
		Log.Print(printerNode.Text);
	}
	public GenericDialogue() : this([]) { }
	public TaskAwaiter<bool> GetAwaiter() => taskCompletionSource.Task.GetAwaiter();
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
	protected override void HandleInput(InputEvent @event)
	{
		if (@event.IsPressed() && !@event.IsEcho()) TryNext();
	}
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
			Close();
			taskCompletionSource.TrySetResult(true);
		}
	}
}
