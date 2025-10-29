using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using RealismCombat.Extensions;
using RealismCombat.Nodes.Components;
namespace RealismCombat.Nodes.Dialogues;
public partial class GenericDialogue : Control
{
	public static GenericDialogue Create()
	{
		var instance = GD.Load<PackedScene>(ResourceTable.dialoguesGenericdialogue).Instantiate<GenericDialogue>();
		return instance;
	}
	[Export(hint: PropertyHint.Range, hintString: "0,5")]
	public float autoContinue;
	readonly TaskCompletionSource<object?> completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
	[Export] PrinterLabelNode printer = null!;
	[Export] TextureRect triangle = null!;
	double continueTimer;
	bool anyKeyToContinue;
	double time;
	bool completed;
	public string Text
	{
		get => printer.Text;
		set => printer.Text = value;
	}
	public event Action? OnDestroy;
	public override void _Ready() => GrabFocus();
	public override void _Input(InputEvent @event)
	{
		if (anyKeyToContinue && HasFocus() && @event.IsPressed() && !@event.IsEcho()) RequestClose();
	}
	public override void _Process(double delta)
	{
		time += delta;
		if (printer.Printing)
		{
			triangle.Visible = true;
			continueTimer = 0;
			if (HasFocus())
			{
				if (Input.IsAnythingPressed())
				{
					printer.speedUp = true;
					triangle.Position = Vector2.Down;
					triangle.SelfModulate = GameColors.activeControl;
				}
				else
				{
					printer.speedUp = false;
					triangle.Position = default;
					triangle.SelfModulate = GameColors.normalControl;
				}
			}
			else
			{
				triangle.Position = default;
				triangle.SelfModulate = GameColors.normalControl;
			}
			return;
		}
		if (autoContinue > 0)
		{
			triangle.Visible = false;
			continueTimer += delta;
			if (continueTimer > autoContinue) RequestClose();
			return;
		}
		triangle.Visible = true;
		triangle.SelfModulate = GameColors.normalControl;
		triangle.Position = default;
		anyKeyToContinue = true;
	}
	public override void _ExitTree()
	{
		Complete();
		base._ExitTree();
	}
	public TaskAwaiter<object?> GetAwaiter() => completionSource.Task.GetAwaiter();
	void RequestClose()
	{
		if (IsQueuedForDeletion())
		{
			Complete();
			return;
		}
		Complete();
		QueueFree();
	}
	void Complete()
	{
		if (completed) return;
		completed = true;
		OnDestroy?.TryInvoke();
		completionSource.TrySetResult(null);
	}
}
