using System;
using Godot;
using RealismCombat.Extensions;
namespace RealismCombat.Nodes.Dialogues;
public abstract partial class BaseDialogue : PanelContainer
{
	public event Action<BaseDialogue>? OnDisposing;
	public void Close() => QueueFree();
	public virtual void HandleInput(InputEvent @event) { }
	protected override void Dispose(bool disposing)
	{
		if (disposing) OnDisposing.TryInvoke(this);
		base.Dispose(disposing);
	}
}
