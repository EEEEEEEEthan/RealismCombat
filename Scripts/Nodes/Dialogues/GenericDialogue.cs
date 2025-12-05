using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
[Tool, GlobalClass,]
public partial class GenericDialogue : BaseDialogue
{
	readonly List<string> texts = [];
	readonly TaskCompletionSource tcsDestroyed = new();
	TaskCompletionSource? tcsPrintDone;
	int currentTextIndex = -1;
	Printer printer;
	TextureRect icon;
	VBoxContainer container;
	double time;
	bool keyDown;
	public Task PrintDone => (tcsPrintDone ??= new()).Task;
	public GenericDialogue(IEnumerable<string> initialTexts)
	{
		container = new();
		container.Name = "VBoxContainer";
		AddChild(container);
		{
			printer = new();
			container.AddChild(printer);
			printer.SizeFlagsVertical = SizeFlags.ExpandFill;
		}
		{
			icon = new();
			icon.Name = "Icon";
			container.AddChild(icon);
			icon.Texture = SpriteTable.arrowDown;
			icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		}
		texts.AddRange(initialTexts);
		printer.VisibleCharacters = 0;
		if (texts.Count > 0)
		{
			currentTextIndex = 0;
			printer.Text = texts[0];
			Log.Print(printer.Text);
		}
		else
		{
			printer.Text = string.Empty;
		}
	}
	public GenericDialogue() : this([]) { }
	public override void _Process(double delta)
	{
		if (keyDown && Input.IsAnythingPressed())
			printer.interval = 0;
		else
			printer.interval = 0.1f;
		if (printer.Printing || string.IsNullOrEmpty(printer.Text))
		{
			icon.SelfModulate = GameColors.transparent;
		}
		else
		{
			time += delta;
			icon.SelfModulate = time > 0.5 ? GameColors.grayGradient[^1] : GameColors.transparent;
			if (time > 1) time = 0;
		}
		if (LaunchArgs.port != null)
		{
			printer.interval = 0;
			TryNext();
		}
	}
	/// <summary>
	///     追加新的文本内容
	/// </summary>
	/// <param name="text">要追加的文本</param>
	public void AddText(string text)
	{
		if (string.IsNullOrEmpty(text)) return;
		texts.Add(text);
		if (currentTextIndex >= 0) return;
		currentTextIndex = 0;
		printer.Text = text;
		printer.VisibleCharacters = 0;
		Log.Print(text);
	}
	public TaskAwaiter GetAwaiter() => tcsDestroyed.Task.GetAwaiter();
	protected override void HandleInput(InputEvent @event)
	{
		if (@event.IsPressed() && !@event.IsEcho())
			if (TryNext())
				keyDown = false;
			else
				keyDown = true;
	}
	bool TryNext()
	{
		if (currentTextIndex < 0) return false;
		if (printer.Printing) return false;
		currentTextIndex++;
		if (currentTextIndex < texts.Count)
		{
			printNext();
		}
		else
		{
			var taskCompletionSource = tcsPrintDone;
			tcsPrintDone = null;
			taskCompletionSource?.TrySetResult();
			if (currentTextIndex < texts.Count)
			{
				printNext();
			}
			else
			{
				Close();
				tcsDestroyed.SetResult();
			}
		}
		return true;
		void printNext()
		{
			var txt = texts[currentTextIndex];
			printer.Text += "\n" + txt;
			Log.Print(txt);
		}
	}
}
