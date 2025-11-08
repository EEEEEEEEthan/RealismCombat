using System;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
public abstract partial class BaseDialogue : PanelContainer
{
	bool hasClosed;
	public event Action<BaseDialogue>? OnDisposing;
	protected BaseDialogue()
	{
		CustomMinimumSize = new(128, 96);
		SetAnchorsPreset(LayoutPreset.BottomWide);
		SetOffset(Side.Left, 0);
		SetOffset(Side.Right, 0);
		SetOffset(Side.Bottom, 0);
		SetOffset(Side.Top, -CustomMinimumSize.Y);
	}
	public virtual void HandleInput(InputEvent @event) { }
	protected void Close()
	{
		if (hasClosed) return;
		hasClosed = true;
		OnDisposing.TryInvoke(this);
		QueueFree();
	}
	protected override void Dispose(bool disposing)
	{
		if (disposing && !hasClosed)
		{
			hasClosed = true;
			OnDisposing.TryInvoke(this);
		}
		base.Dispose(disposing);
	}
}
