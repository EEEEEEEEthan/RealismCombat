using Godot;
namespace RealismCombat.Nodes.Dialogues;
public abstract partial class BaseDialogue : PanelContainer
{
	public void Close()
	{
		DialogueManager.RemoveDialogue(this);
		QueueFree();
	}
	public virtual void HandleInput(InputEvent @event) { }
}
