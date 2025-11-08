using System.Collections.Generic;
using Godot;
using RealismCombat.Nodes.Dialogues;
namespace RealismCombat.AutoLoad;
public partial class DialogueManager : Node
{
	static DialogueManager instance = null!;
	public static GenericDialogue CreateGenericDialogue()
	{
		var dialogue = new GenericDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		dialogue.OnDisposing += instance.OnDialogueDisposing;
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue()
	{
		var dialogue = new MenuDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		dialogue.OnDisposing += instance.OnDialogueDisposing;
		return dialogue;
	}
	public static BaseDialogue? GetTopDialogue()
	{
		if (instance.dialogueStack.Count == 0) return null;
		return instance.dialogueStack[^1];
	}
	public static int GetDialogueCount() => instance.dialogueStack.Count;
	static void AddDialogue(BaseDialogue dialogue)
	{
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 添加Dialogue: {dialogue.GetType().Name}, 当前堆栈大小: {instance.dialogueStack.Count}");
	}
	readonly List<BaseDialogue> dialogueStack = [];
	public override void _Ready()
	{
		instance = this;
		Log.Print("[DialogueManager] DialogueManager初始化完成");
	}
	public override void _Input(InputEvent @event)
	{
		var topDialogue = GetTopDialogue();
		topDialogue?.HandleInput(@event);
	}
	void OnDialogueDisposing(BaseDialogue dialogue) => dialogueStack.Remove(dialogue);
}
