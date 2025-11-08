using System;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
public abstract partial class BaseDialogue : PanelContainer
{
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
	public void Close() => QueueFree();
	public virtual void HandleInput(InputEvent @event) { }
	protected override void Dispose(bool disposing)
	{
		if (disposing) OnDisposing.TryInvoke(this);
		base.Dispose(disposing);
	}
}
