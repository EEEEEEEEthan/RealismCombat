using System.Collections.Generic;
using Godot;
namespace RealismCombat.Nodes.Dialogues;
public partial class DialogueManager : Node
{
	static DialogueManager instance = null!;
	public static GenericDialogue CreateGenericDialogue()
	{
		var dialogue = new GenericDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 创建GenericDialogue, 当前堆栈大小: {instance.dialogueStack.Count}");
		return dialogue;
	}
	public static MenuDialogue CreateMenuDialogue()
	{
		var dialogue = new MenuDialogue();
		instance.dialogueStack.Add(dialogue);
		instance.AddChild(dialogue);
		Log.Print($"[DialogueManager] 创建MenuDialogue, 当前堆栈大小: {instance.dialogueStack.Count}");
		return dialogue;
	}
	public static void RemoveDialogue(BaseDialogue dialogue)
	{
		instance.dialogueStack.Remove(dialogue);
		Log.Print($"[DialogueManager] 移除Dialogue: {dialogue.GetType().Name}, 当前堆栈大小: {instance.dialogueStack.Count}");
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
}
