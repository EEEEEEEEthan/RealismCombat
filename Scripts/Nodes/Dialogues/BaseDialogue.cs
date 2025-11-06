using Godot;
namespace RealismCombat.Nodes;
public abstract partial class BaseDialogue : PanelContainer
{
	public bool IsTopDialogue => DialogueManager.IsTopDialogue(this);
	public override void _Input(InputEvent @event)
	{
		if (!IsTopDialogue) return;
		HandleInput(@event);
	}
	public void Close()
	{
		DialogueManager.RemoveDialogue(this);
		QueueFree();
	}
	protected virtual void HandleInput(InputEvent @event) { }
}
