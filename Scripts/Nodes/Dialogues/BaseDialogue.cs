using System;
using Godot;
using RealismCombat.AutoLoad;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
public abstract partial class BaseDialogue : PanelContainer, DialogueManager.IDialogue
{
	bool hasClosed;
	public event Action<BaseDialogue>? OnClosed;
	protected BaseDialogue()
	{
		CustomMinimumSize = new(128, 96);
		SetAnchorsPreset(LayoutPreset.BottomWide);
		SetOffset(Side.Left, 0);
		SetOffset(Side.Right, 0);
		SetOffset(Side.Bottom, 0);
		SetOffset(Side.Top, -CustomMinimumSize.Y);
	}
	protected virtual void HandleInput(InputEvent @event) { }
	protected void Close()
	{
		if (hasClosed) return;
		hasClosed = true;
		OnClosed.TryInvoke(this);
		QueueFree();
	}
	protected override void Dispose(bool disposing)
	{
		if (disposing && !hasClosed)
		{
			hasClosed = true;
			OnClosed.TryInvoke(this);
		}
		base.Dispose(disposing);
	}
	void DialogueManager.IDialogue.HandleInput(InputEvent @event) => HandleInput(@event);
}
